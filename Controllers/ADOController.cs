using CsvHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReestrGNVLS.Data;
using ReestrGNVLS.Extensions;
using ReestrGNVLS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReestrGNVLS.Controllers
{
    public class ADOController : Controller
    {
        List<Reestr> data = new List<Reestr>();
        readonly string logPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "log", "gnvls.log");
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ADOController(IHostingEnvironment appEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _appEnvironment = appEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        private readonly string connectionString = new SqlConnectionStringBuilder
        {
            DataSource = "hidden",
            UserID = "hidden",
            Password = "hidden",
            Pooling = true,
        }.ConnectionString;

        //количество записей выборки
        private async Task<int> GetRecordsCount(string aptekaId, string userString)
        {
            int count = 0;
            string name = string.Empty;
            string barcode = string.Empty;

            SqlCommand query = new SqlCommand();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    query.CommandText = SqlCommandText.ForGetRecordsCount1;

                    if (Regex.IsMatch(userString, @"^\d+$") && userString.Length >= 8)
                    {
                        barcode = userString;
                    }
                    else
                    {
                        name = userString;
                    }

                    query.Connection = connection;
                    //query.Parameters.AddWithValue("aptekaId", int.Parse(aptekaId));
                    //query.Parameters.AddWithValue("regionId", int.Parse(regionId));
                    query.Parameters.AddWithValue("name", "%" + name + "%");
                    query.Parameters.AddWithValue("barcode", "%" + barcode + "%");
                    query.Parameters.AddWithValue("tableName", "tmp_gnvls_" + aptekaId);
                    count = (int)await query.ExecuteScalarAsync();
                }
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
            }

            return count;
        }

        //определение параметров аптеки
        private async Task<List<Apteka>> GetAptekaModel(string aptekaId)
        {
            List<Apteka> data = new List<Apteka>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    SqlCommand query = new SqlCommand
                    {
                        CommandText = $@"select CONVERT(VARCHAR(20), reg.region) RegionId, 
                                               isnull(apt.Region, '') RegionName, 
                                               isnull(cl.naimen, '') FullAptekaName, 
                                               isnull(apt.TaxType, '') TaxType
                                        from opeka_base.dbo.AptInfo apt
                                                left join [vm-sql2008].Ges.dbo.spr_region reg on case
                                                    when apt.Region = 'Республика Мордовия' then 'Республика Мордовия (Саранск)'
                                                    when apt.Region = 'Удмуртия' then 'Удмуртская республика'
                                                    when apt.Region = 'КОМИ (1 зона Сыктывкар)' then 'Республика Коми (1 зона)'
                                                    when apt.Region = 'Республика Саха' then 'Респ. Саха (1 зона)'
                                                    when apt.Region = 'КОМИ (2 зона Сыктывкар)' then 'Республика Коми (2 зона)'
                                                    when apt.Region = 'Марий Эл республика' then 'Республика Марий-Эл'
                                                    when apt.Region = 'Красноярский край (зона 3)' then 'Красноярский край (3 зона)'
                                                    when apt.Region = 'Красноярский край (зона 1)' then 'Красноярский край (1 зона)'
                                                    when apt.Region = 'Москва (город)' then 'г.Москва'
                                                    when apt.Region = 'ХМАО (Сургут)' then 'Ханты-Мансийский автономный округ'
                                                    when apt.Region = 'Санкт-Петербург (город)' then 'г.Санкт-Петербург'
                                                    when apt.Region = 'ЯНАО (2 зона Уренгой)' then 'Ямало-Ненецкий автономный округ(2 зона)'
                                                    when apt.Region = 'Татарстан' then 'Республика Татарстан'
                                                    when apt.Region = 'ЯНАО (1 зона Уренгой) ' then 'Ямало-Ненецкий автономный округ(1 зона)'
                                                    when apt.Region = 'Башкирия' then 'Республика Башкортостан'
                                                    else apt.Region end = reg.name_reg
                                                left join [Servsql].admzakaz.dbo.clients cl on cl.kp = apt.IDApt
                                        where apt.idapt = @aptekaId;",
                        Connection = connection
                    };
                    query.Parameters.AddWithValue("aptekaId", int.Parse(aptekaId));
                    SqlDataReader reader = await query.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        Apteka row = new Apteka
                        {
                            RegionId = reader["RegionId"] as string,
                            RegionName = reader["RegionName"] as string,
                            FullAptekaName = reader["FullAptekaName"] as string,
                            TaxType = reader["TaxType"] as string
                        };
                        if (string.IsNullOrEmpty(row.RegionId))
                        {
                            throw new Exception("Ошибка! Не найден регион для аптеки apteka_id = " + aptekaId);
                        }
                        data.Add(row);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + Environment.NewLine + e.StackTrace);
            }
            return data;
        }

        //проверки наличия аутентификации пользователя
        public bool IsAuthenticated()
        {
            var User = HttpContext.User;
            return User.Identities.Any(x => x.IsAuthenticated);
        }

        //выборка данных для csv файла
        [HttpGet]
        public async Task<IEnumerable<Reestr>> GetCsvData()
        {
            string aptekaId = HttpContext.Session.GetString("aptekaId");
            Apteka aptekaModel = HttpContext.Session.Get<Apteka>("aptekaModel");

            string name = string.Empty;
            string barcode = string.Empty;
            if (HttpContext.Session.Keys.Contains("userString"))
            {
                string userString = HttpContext.Session.GetString("userString");
                if (Regex.IsMatch(userString, @"^\d+$") && userString.Length >= 8)
                {
                    barcode = userString;
                }
                else
                {
                    name = userString;
                }
            }

            SqlCommand query = new SqlCommand();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                query.CommandText = SqlCommandText.ForCsv1;
                query.Connection = connection;
                //query.Parameters.AddWithValue("aptekaId", int.Parse(aptekaId));
                //query.Parameters.AddWithValue("regionId", int.Parse(aptekaModel.RegionId));
                query.Parameters.AddWithValue("name", "%" + name + "%");
                query.Parameters.AddWithValue("barcode", "%" + barcode + "%");
                query.Parameters.AddWithValue("tableName", "tmp_gnvls_" + aptekaId);
                SqlDataReader reader = await query.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        Reestr row = new Reestr
                        {
                            FullAptekaName = reader["FullAptekaName"] as string,
                            Mhh = reader["Mhh"] as string,
                            Series = reader["Series"] as string,
                            Name = reader["Name"] as string,
                            Pro = reader["Pro"] as string,
                            Barcode = reader["Barcode"] as string,
                            Nds = reader["Nds"] as string,
                            ProducerRegisteredPrice = reader["ProducerRegisteredPrice"] as string,
                            ProducerRealPrice = reader["ProducerRealPrice"] as string,
                            PurchasePriceWithoutVAT = reader["PurchasePriceWithoutVAT"] as string,
                            PremiumInPercentOpt = reader["PremiumInPercentOpt"] as string,
                            PremiumInRubOpt = reader["PremiumInRubOpt"] as string,
                            MaxOptPercent = reader["MaxOptPercent"] as string,
                            PurchasePrice = reader["PurchasePrice"] as string,
                            RetailPriceWithoutVAT = reader["RetailPriceWithoutVAT"] as string,
                            PremiumInPercentRetail = reader["PremiumInPercentRetail"] as string,
                            PremiumInRubRetail = reader["PremiumInRubRetail"] as string,
                            MaxRetailPercent = reader["MaxRetailPercent"] as string,
                            RetailPrice = reader["RetailPrice"] as string
                        };
                        data.Add(row);
                    }
                }
                reader.Close();
            }
            return data;
        }

        //формирование и отправка пользователю файла csv
        [HttpGet]
        public async Task<IActionResult> DownloadScv()
        {
            if (!IsAuthenticated())
            {
                HttpContext.Session.Remove("aptekaModel");
                HttpContext.Session.Remove("aptekaId");
                HttpContext.Session.Remove("userString");
                return View("Logout");
            }

            //string path = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "files", "gnvls.csv");
            try
            {
                IEnumerable<Reestr> data = await GetCsvData();

                using (MemoryStream memoryStream = new MemoryStream())
                //using (StreamWriter writer = new StreamWriter(path, false, Encoding.GetEncoding(1251), 1024))
                using (StreamWriter writer = new StreamWriter(memoryStream, Encoding.GetEncoding(1251)))
                using (CsvWriter csvWriter = new CsvWriter(writer))
                {
                    writer.AutoFlush = true; //автоматом сбрасывает буффер в поток, при каждом вызове Write
                    csvWriter.Configuration.Delimiter = ";";
                    csvWriter.Configuration.HasHeaderRecord = true;
                    csvWriter.Configuration.RegisterClassMap<ReestrClassMap>();
                    csvWriter.WriteHeader<Reestr>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecords(data);
                    memoryStream.Position = 0;
                    return File(memoryStream.ToArray(), "application/octet-stream", "gnvls.csv");
                }
                //return PhysicalFile(path, "application/octet-stream", "Files/gnvls.csv");
            }
            catch (SqlException e)
            {
                //для заполнения шапки страницы получаем из сессии модель аптеки и ее id
                if (HttpContext.Session.Keys.Contains("aptekaModel"))
                {
                    Apteka aptekaModel = HttpContext.Session.Get<Apteka>("aptekaModel");
                    ViewBag.FullAptekaName = aptekaModel.FullAptekaName;
                    ViewBag.RegionName = aptekaModel.RegionName;
                    ViewBag.TaxType = aptekaModel.TaxType;
                }
                ViewBag.aptekaId = HttpContext.Session.GetString("aptekaId");

                ViewBag.Message = "Ошибка получения данных с сервера, обновите страницу и попробуйте повторно выгрузить данные";
                WebExtensions.WriteToLog(logPath, DateTime.Now + " " + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
                return View("Index", data);
            }
            catch (Exception e)
            {
                //для заполнения шапки страницы получаем из сессии модель аптеки и ее id
                if (HttpContext.Session.Keys.Contains("aptekaModel"))
                {
                    Apteka aptekaModel = HttpContext.Session.Get<Apteka>("aptekaModel");
                    ViewBag.FullAptekaName = aptekaModel.FullAptekaName;
                    ViewBag.RegionName = aptekaModel.RegionName;
                    ViewBag.TaxType = aptekaModel.TaxType;
                }
                ViewBag.aptekaId = HttpContext.Session.GetString("aptekaId");

                ViewBag.Message = "Произошла ошибка в работе приложения, обновите страницу";
                WebExtensions.WriteToLog(logPath, DateTime.Now + " " + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
                return View("Index", data);
            }
        }

        //автоматическая аутентификация пользователя с ипользованием cookie
        public async Task AutoLoginByCookie(string aptekaId)
        {
            // Clear the existing external cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("aptekaModel");
            HttpContext.Session.Remove("aptekaId");
            HttpContext.Session.Remove("userString");

            HttpContext.Session.SetString("aptekaId", aptekaId);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, aptekaId),
                    new Claim(ClaimTypes.Role, "User"),
                };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(720),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                IsPersistent = false,
                // Whether the authentication session is persisted across 
                // multiple requests. Required when setting the 
                // ExpireTimeSpan option of CookieAuthenticationOptions 
                // set with AddCookie. Also required when setting 
                // ExpiresUtc.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        //выборка данных и занесение их во временную таблицу, с которой далее будет работать приложение (для ускорения выполнения запросов)
        public async Task CreateTempDataTable(string aptekaId, string RegionId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    SqlCommand query = new SqlCommand();
                    query.CommandText = SqlCommandText.ForCreateTempDataTable1;
                    query.Connection = connection;
                    query.Parameters.AddWithValue("aptekaId", int.Parse(aptekaId));
                    query.Parameters.AddWithValue("regionId", int.Parse(RegionId));
                    query.Parameters.AddWithValue("tableName", "tmp_gnvls_" + aptekaId);

                    await query.ExecuteNonQueryAsync();
                }
                catch (SqlException)
                {
                    string message = DateTime.Now + " Произошла ошибка при создании временной таблицы " + Environment.NewLine;
                    WebExtensions.WriteToLog(logPath, message);
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        //главная страница сайта
        public async Task<IActionResult> Index()
        {
            //если в браузере есть куки с ключом aptekaId, то выполняем автоматическую аутентификацию используя этот идентификатор aptekaId
            if (HttpContext.Request.Cookies.ContainsKey("aptekaId") && !IsAuthenticated())
            {
                string _aptekaId = HttpContext.Request.Cookies["aptekaId"];
                await AutoLoginByCookie(_aptekaId);
                return RedirectToAction("Index");
            }

            if (!IsAuthenticated())
            {
                HttpContext.Session.Remove("aptekaModel");
                HttpContext.Session.Remove("aptekaId");
                HttpContext.Session.Remove("userString");
                return View("Logout");
            }

            //определяем адрес на который пришел запрос gv.qwerty.plus или gnvls.qwerty.plus
            string requestUrl = HttpContext.Request.Host.Host;
            ViewBag.requestUrl = requestUrl;
            //ViewBag.requestUrl = "gv.qwerty.plus";
            //requestUrl = "gv.qwerty.plus";

            //параметры для постраничной навигации
            int offset = 0; //default value
            int rowsCount = 20; //default value

            //id аптеки получаем из сессии, значение сохранено в сессию при аутентификации
            string aptekaId = HttpContext.Session.GetString("aptekaId");

            //проверяем наличие значения id аптеки, если его нет, то отправляем на страницу аутентификации
            if (string.IsNullOrEmpty(aptekaId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                //return RedirectToPage("/Account/Login");
                return View("Logout");
            }

            //сохраняем id аптеки в куки на стороне клиента, используется для автоматической аутентификации
            //if (!HttpContext.Request.Cookies.ContainsKey("aptekaId") || HttpContext.Request.Cookies["aptekaId"] != aptekaId)
            //{
            //}
            var cookieOptions = new CookieOptions()
            {
                Path = "/",
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMonths(1) //срок жизни куки атоматической авторизации
            };
            HttpContext.Response.Cookies.Append("aptekaId", aptekaId, cookieOptions);

            ViewBag.aptekaId = aptekaId;
            Apteka aptekaModel = null;

            //заносим модель аптеки в сессию
            if (HttpContext.Session.Keys.Contains("aptekaModel"))
            {
                aptekaModel = HttpContext.Session.Get<Apteka>("aptekaModel");
            }
            else
            {
                List<Apteka> apteka = await GetAptekaModel(aptekaId);
                HttpContext.Session.Set("aptekaModel", apteka.First());
            }

            //срабатывает только при первом обращении к контроллеру, когда модели аптеки еще нет в сессии
            if (aptekaModel == null)
            {
                aptekaModel = HttpContext.Session.Get<Apteka>("aptekaModel");
            }

            ViewBag.RegionId = aptekaModel.RegionId;
            ViewBag.RegionName = aptekaModel.RegionName;
            ViewBag.FullAptekaName = aptekaModel.FullAptekaName;
            ViewBag.TaxType = aptekaModel.TaxType;

            string userString = string.Empty;

            if (Request.Method == "POST")
            {
                //сохраняем строку поиска в сессию, используется для выгрузки данных в csv
                if (Request.Form.Keys.Contains("userString"))
                {
                    userString = Request.Form["userString"];
                    HttpContext.Session.SetString("userString", userString);
                }
            }

            try
            {
                //создаем временную таблицу с результатами
                await CreateTempDataTable(aptekaId, aptekaModel.RegionId);

                //определяем количество страниц для навигации сайта
                int pagesCount = await GetRecordsCount(aptekaId, userString);

                if (Request.Method == "GET")
                {
                    pagesCount = (int)Math.Ceiling((double)pagesCount / rowsCount);
                    ViewBag.PagesCount = pagesCount;
                }

                if (Request.Method == "POST")
                {
                    int currentPage = 1;

                    if (!string.IsNullOrEmpty(Request.Form["currentPage"]))
                    {
                        currentPage = int.Parse(Request.Form["currentPage"]);
                    }

                    if (!string.IsNullOrEmpty(Request.Form["rowsCount"]))
                    {
                        rowsCount = int.Parse(Request.Form["rowsCount"]);
                    }
                    pagesCount = (int)Math.Ceiling((double)pagesCount / rowsCount);
                    ViewBag.PagesCount = pagesCount;

                    if (!string.IsNullOrEmpty(Request.Form["direction"]))
                    {
                        switch (Request.Form["direction"])
                        {
                            case "next":
                                if (currentPage < pagesCount)
                                {
                                    currentPage++;
                                }
                                offset = (currentPage - 1) * rowsCount;
                                break;
                            case "prev":
                                if (currentPage > 1)
                                {
                                    currentPage--;
                                }
                                offset = (currentPage - 1) * rowsCount;
                                break;
                            case "first":
                                offset = 0;
                                break;
                            case "last":
                                offset = (pagesCount - 1) * rowsCount;
                                break;
                            case "select-page":
                                offset = (currentPage - 1) * rowsCount;
                                break;
                            case "page-size":
                                offset = (currentPage - 1) * rowsCount;
                                break;
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string name = string.Empty;
                    string barcode = string.Empty;

                    if (Regex.IsMatch(userString, @"^\d+$") && userString.Length >= 8)
                    {
                        barcode = userString;
                    }
                    else
                    {
                        name = userString;
                    }

                    SqlCommand query = new SqlCommand();
                    query.CommandText = SqlCommandText.ForSite1;
                    query.Connection = connection;
                    query.Parameters.AddWithValue("offset", offset);
                    query.Parameters.AddWithValue("rowsCount", rowsCount);
                    //query.Parameters.AddWithValue("aptekaId", int.Parse(aptekaId));
                    //query.Parameters.AddWithValue("regionId", int.Parse(aptekaModel.RegionId));
                    query.Parameters.AddWithValue("name", "%" + name + "%");
                    query.Parameters.AddWithValue("barcode", "%" + barcode + "%");
                    query.Parameters.AddWithValue("tableName", "tmp_gnvls_" + aptekaId);

                    SqlDataReader reader = await query.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            Reestr row = new Reestr
                            {
                                FullAptekaName = reader["FullAptekaName"] as string,
                                Mhh = reader["Mhh"] as string,
                                Series = reader["Series"] as string,
                                Name = reader["Name"] as string,
                                Pro = reader["Pro"] as string,
                                Barcode = reader["Barcode"] as string,
                                Nds = reader["Nds"] as string,
                                ProducerRegisteredPrice = reader["ProducerRegisteredPrice"] as string,
                                ProducerRealPrice = reader["ProducerRealPrice"] as string,
                                PurchasePriceWithoutVAT = reader["PurchasePriceWithoutVAT"] as string,
                                PremiumInPercentOpt = reader["PremiumInPercentOpt"] as string,
                                PremiumInRubOpt = reader["PremiumInRubOpt"] as string,
                                MaxOptPercent = reader["MaxOptPercent"] as string,
                                PurchasePrice = reader["PurchasePrice"] as string,
                                RetailPriceWithoutVAT = reader["RetailPriceWithoutVAT"] as string,
                                PremiumInPercentRetail = reader["PremiumInPercentRetail"] as string,
                                PremiumInRubRetail = reader["PremiumInRubRetail"] as string,
                                MaxRetailPercent = reader["MaxRetailPercent"] as string,
                                RetailPrice = reader["RetailPrice"] as string
                            };
                            data.Add(row);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Запрошенная информация не найдена";
                    }
                    reader.Close();
                }
            }
            catch (SqlException e)
            {
                ViewBag.Message = "Ошибка получения данных с сервера, обновите страницу";
                WebExtensions.WriteToLog(logPath, DateTime.Now + " " + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
            }
            catch (Exception e)
            {
                ViewBag.Message = "Произошла критическая ошибка в работе приложения, пожалуйста перезагрузите ваш браузер, если ошибка будет повторяться - обратитесь к разработчику сайта";
                WebExtensions.WriteToLog(logPath, DateTime.Now + " " + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
            }

            if (Request.Method == "POST")
            {
                if (requestUrl == "gv.qwerty.plus")
                {
                    return PartialView("_IndexGv", data);
                }
                return PartialView("_Index", data);
            }

            if (requestUrl == "gv.qwerty.plus")
            {
                return View("IndexGv", data);
            }
            return View("Index", data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}