using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReestrGNVLS.Data
{
    public sealed class SqlCommandText
    {
        public static readonly string ForSite = @"with res as (select apt.*, reg.O50_B, reg.O500_B, reg.O501_B, reg.R50_B, reg.R500_B, reg.R501_B
             from opeka_base.dbo.AptInfo apt
                    left join [vm-sql2008].[Ges].[dbo].[spr_region] reg on case
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
             where apt.idapt = @aptekaId),
     res0 as (SELECT ROW_NUMBER() over (partition by ean13, cen order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     r as (select * from res0 where id = 1),
     res0barcode as (SELECT ROW_NUMBER() over (partition by ean13 order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rbarcode as (select * from res0barcode where id = 1),
     res0IdposIdpro as (SELECT ROW_NUMBER() over (partition by idpos, idpro order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdposIdpro as (select * from res0IdposIdpro where id = 1),
     res0Idpos as (SELECT ROW_NUMBER() over (partition by idpos order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdpos as (select * from res0Idpos where id = 1),
     res1 as (select bal.IDApt                                                  IDApt,
                     apt.TaxType                                                TaxType,
                     apt.O50_B                                                  O50_B,
                     apt.O500_B                                                 O500_B,
                     apt.O501_B                                                 O501_B,
                     apt.R50_B                                                  R50_B,
                     apt.R500_B                                                 R500_B,
                     apt.R501_B                                                 R501_B,
                     r.region                                                   region,
                     cl.naimen                                                  FullAptekaName,
                     coalesce(r.int, rbarcode.int, rIdposIdpro.int, rIdpos.int)     Mhh,
                     coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name) name,
                     coalesce(r.opis, rbarcode.opis, rIdposIdpro.opis, rIdpos.opis) opis,
                     coalesce(r.pro, rbarcode.pro, rIdposIdpro.pro, rIdpos.pro)     Pro,
                     bal.BarCode                                                Barcode,
                     bal.VAT                                                    Nds,
                     bal.ProducerRegisteredPrice                                ProducerRegisteredPrice,
                     bal.ProducerRealPrice                                      ProducerRealPrice,
                     bal.PurchasePriceWithoutVAT                                PurchasePriceWithoutVAT,
                     bal.PurchasePrice                                          PurchasePrice,
                     case
                       when apt.TaxType = 'ТСН' then ROUND(bal.Price * 100 / (100 + bal.VAT) + 0.00499999, 2)
                       else bal.Price end                                       RetailPriceWithoutVAT,
                     bal.Price                                                  RetailPrice,
                     opeka_base.dbo.fnMathMin(case
                                                when bal.ProducerRegisteredPrice = 0 then bal.ProducerRealPrice
                                                else bal.ProducerRegisteredPrice end,
                                              case
                                                when bal.ProducerRealPrice = 0 then bal.ProducerRegisteredPrice
                                                else bal.ProducerRealPrice end) MinProducerPrice
              from opeka_base.dbo.GD_StockBalance bal
                     left join r on bal.BarCode = r.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS and
                                    bal.ProducerRegisteredPrice = r.cen
                     left join rbarcode on bal.BarCode = rbarcode.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS
                     left join rIdposIdpro on bal.IDPos = rIdposIdpro.idpos and bal.IDPro = rIdposIdpro.idpro
                     left join rIdpos on bal.IDPos = rIdpos.idpos
                     left join res apt on apt.idapt = bal.IDApt
                     left join [Servsql].admzakaz.dbo.clients cl on cl.kp = bal.IDApt
              where bal.IDApt = @aptekaId
                and bal.IsLive = 1 and not coalesce(bal.properties,'') like '%карантин%'
                and not coalesce(bal.Properties, '') like '%apteka.ru%' and not coalesce(bal.Properties, '') like '%ZS%'
                and bal.Quantity > bal.Reserved
                and coalesce(r.int, rbarcode.int, rIdposIdpro.int, rIdpos.int) + coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name) +
                    coalesce(r.opis, rbarcode.opis, rIdposIdpro.opis, rIdpos.opis) + coalesce(r.pro, rbarcode.pro, rIdposIdpro.pro, rIdpos.pro) like @name
                 and bal.BarCode like @barcode
              ORDER by coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name)
              OFFSET @offset rows
              FETCH NEXT @rowsCount ROWS ONLY
),
     res2 as (select case
                       when res1.MinProducerPrice <= 50 then res1.O50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.O500_B
                       when res1.MinProducerPrice > 500 then res1.O501_B
                         end MaxOptPercent,
                     case
                       when res1.MinProducerPrice <= 50 then res1.R50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.R500_B
                       when res1.MinProducerPrice > 500 then res1.R501_B
                         end MaxRetailPercent,
                     res1.*
              from res1)
select isnull(res2.FullAptekaName, '')                                                                                                              FullAptekaName,
       isnull(res2.Mhh, '')                                                                                                                         Mhh,
       isnull(res2.name, '') + ' ' + isnull(res2.opis, '')                                                                                          Name,
       isnull(res2.Pro, '')                                                                                                                         Pro,
       isnull(res2.BarCode, '')                                                                                                                     Barcode,
       isnull(CONVERT(VARCHAR(3), CONVERT(float, res2.Nds)), '0')                                                                                   Nds,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRegisteredPrice)), '0')                                                              ProducerRegisteredPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRealPrice)), '0')                                                                    ProducerRealPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxOptPercent)), '0')                                                                        MaxOptPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT)), '0')                                                              PurchasePriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((res2.PurchasePriceWithoutVAT - res2.MinProducerPrice) * 100 / res2.MinProducerPrice + 0.00499999, 2))), '0') PremiumInPercentOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT - res2.MinProducerPrice)), '0')                                      PremiumInRubOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePrice)), '0')                                                                        PurchasePrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT)), '0')                                                                RetailPriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((((res2.RetailPriceWithoutVAT - res2.PurchasePrice) * 100) /
                                       res2.MinProducerPrice) + 0.00499999, 2))), '0')                                                              PremiumInPercentRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT - res2.PurchasePrice)), '0')                                           PremiumInRubRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxRetailPercent)), '0')                                                                     MaxRetailPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPrice)), '0')                                                                          RetailPrice
from res2 order by Name;";

        public static readonly string ForSite1 = @"USE tempdb
declare @sqlQuery nvarchar(1000);
declare @ParmDefinition nvarchar(500);
set @ParmDefinition = N'@name nvarchar(1000), @barcode varchar(30), @offset int, @rowsCount int';
set @sqlQuery = 'select *
from tempdb.dbo.'+@tableName+' bal
where bal.Mhh + bal.Name + bal.Pro + bal.Series COLLATE SQL_Latin1_General_CP1251_CI_AS like @name
    and bal.Barcode + bal.Series like @barcode
ORDER by bal.Name OFFSET @offset rows
FETCH NEXT @rowsCount ROWS ONLY'
EXEC sp_executesql @sqlQuery, @ParmDefinition, @name = @name, @barcode = @barcode, @offset = @offset, @rowsCount = @rowsCount;";

        public static readonly string ForGetRecordsCount = @"with res0 as (SELECT ROW_NUMBER() over (partition by ean13, cen order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
                                        r as (select * from res0 where id = 1)
                                select count(*)
                                from opeka_base.dbo.GD_StockBalance bal
                                        left join r on bal.BarCode = r.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS and
                                                        bal.ProducerRegisteredPrice = r.cen
                                where bal.IDApt = @aptekaId
                                    and bal.IsLive = 1 and not coalesce(bal.properties,'') like '%карантин%'
                                    and not coalesce(bal.Properties, '') like '%apteka.ru%' and not coalesce(bal.Properties, '') like '%ZS%'
                                    and bal.Quantity > bal.Reserved
                                    and r.int + r.name + r.opis + r.pro like @name
                                    and bal.BarCode like @barcode;";

        public static readonly string ForGetRecordsCount1 = @"USE tempdb 
declare @sqlQuery nvarchar(1000);
declare @ParmDefinition nvarchar(500);
set @ParmDefinition = N'@name nvarchar(1000), @barcode varchar(30)';
set @sqlQuery = 'select count(*) from tempdb.dbo.'+@tableName+' bal
where bal.Mhh + bal.Name + bal.Pro + bal.Series COLLATE SQL_Latin1_General_CP1251_CI_AS like @name
and bal.Barcode + bal.Series like @barcode'
EXEC sp_executesql @sqlQuery, @ParmDefinition, @name = @name, @barcode = @barcode;";

        public static readonly string ForCsv = @"with res as (select apt.*, reg.O50_B, reg.O500_B, reg.O501_B, reg.R50_B, reg.R500_B, reg.R501_B
             from opeka_base.dbo.AptInfo apt
                    left join [vm-sql2008].[Ges].[dbo].[spr_region] reg on case
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
             where apt.idapt = @aptekaId),
     res0 as (SELECT ROW_NUMBER() over (partition by ean13, cen order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     r as (select * from res0 where id = 1),
     res0barcode as (SELECT ROW_NUMBER() over (partition by ean13 order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rbarcode as (select * from res0barcode where id = 1),
     res0IdposIdpro as (SELECT ROW_NUMBER() over (partition by idpos, idpro order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdposIdpro as (select * from res0IdposIdpro where id = 1),
     res0Idpos as (SELECT ROW_NUMBER() over (partition by idpos order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdpos as (select * from res0Idpos where id = 1),
     res1 as (select bal.IDApt                                                  IDApt,
                     apt.TaxType                                                TaxType,
                     apt.O50_B                                                  O50_B,
                     apt.O500_B                                                 O500_B,
                     apt.O501_B                                                 O501_B,
                     apt.R50_B                                                  R50_B,
                     apt.R500_B                                                 R500_B,
                     apt.R501_B                                                 R501_B,
                     r.region                                                   region,
                     cl.naimen                                                  FullAptekaName,
                     coalesce(r.int, rbarcode.int, rIdposIdpro.int, rIdpos.int)     Mhh,
                     coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name) name,
                     coalesce(r.opis, rbarcode.opis, rIdposIdpro.opis, rIdpos.opis) opis,
                     coalesce(r.pro, rbarcode.pro, rIdposIdpro.pro, rIdpos.pro)     Pro,
                     bal.BarCode                                                Barcode,
                     bal.VAT                                                    Nds,
                     bal.ProducerRegisteredPrice                                ProducerRegisteredPrice,
                     bal.ProducerRealPrice                                      ProducerRealPrice,
                     bal.PurchasePriceWithoutVAT                                PurchasePriceWithoutVAT,
                     bal.PurchasePrice                                          PurchasePrice,
                     case
                       when apt.TaxType = 'ТСН' then ROUND(bal.Price * 100 / (100 + bal.VAT) + 0.00499999, 2)
                       else bal.Price end                                       RetailPriceWithoutVAT,
                     bal.Price                                                  RetailPrice,
                     opeka_base.dbo.fnMathMin(case
                                                when bal.ProducerRegisteredPrice = 0 then bal.ProducerRealPrice
                                                else bal.ProducerRegisteredPrice end,
                                              case
                                                when bal.ProducerRealPrice = 0 then bal.ProducerRegisteredPrice
                                                else bal.ProducerRealPrice end) MinProducerPrice
              from opeka_base.dbo.GD_StockBalance bal
                     left join r on bal.BarCode = r.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS and
                                    bal.ProducerRegisteredPrice = r.cen
                     left join rbarcode on bal.BarCode = rbarcode.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS
                     left join rIdposIdpro on bal.IDPos = rIdposIdpro.idpos and bal.IDPro = rIdposIdpro.idpro
                     left join rIdpos on bal.IDPos = rIdpos.idpos
                     left join res apt on apt.idapt = bal.IDApt
                     left join [Servsql].admzakaz.dbo.clients cl on cl.kp = bal.IDApt
              where bal.IDApt = @aptekaId
                and bal.IsLive = 1 and not coalesce(bal.properties,'') like '%карантин%'
                and not coalesce(bal.Properties, '') like '%apteka.ru%' and not coalesce(bal.Properties, '') like '%ZS%'
                and bal.Quantity > bal.Reserved
                and coalesce(r.int, rbarcode.int, rIdposIdpro.int, rIdpos.int) + coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name) +
                    coalesce(r.opis, rbarcode.opis, rIdposIdpro.opis, rIdpos.opis) + coalesce(r.pro, rbarcode.pro, rIdposIdpro.pro, rIdpos.pro) like @name
                 and bal.BarCode like @barcode),
     res2 as (select case
                       when res1.MinProducerPrice <= 50 then res1.O50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.O500_B
                       when res1.MinProducerPrice > 500 then res1.O501_B
                         end MaxOptPercent,
                     case
                       when res1.MinProducerPrice <= 50 then res1.R50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.R500_B
                       when res1.MinProducerPrice > 500 then res1.R501_B
                         end MaxRetailPercent,
                     res1.*
              from res1)
select isnull(res2.FullAptekaName, '')                                                                                                              FullAptekaName,
       isnull(res2.Mhh, '')                                                                                                                         Mhh,
       isnull(res2.name, '') + ' ' + isnull(res2.opis, '')                                                                                          Name,
       isnull(res2.Pro, '')                                                                                                                         Pro,
       isnull(res2.BarCode, '')                                                                                                                     Barcode,
       isnull(CONVERT(VARCHAR(3), CONVERT(float, res2.Nds)), '0')                                                                                   Nds,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRegisteredPrice)), '0')                                                              ProducerRegisteredPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRealPrice)), '0')                                                                    ProducerRealPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxOptPercent)), '0')                                                                        MaxOptPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT)), '0')                                                              PurchasePriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((res2.PurchasePriceWithoutVAT - res2.MinProducerPrice) * 100 / res2.MinProducerPrice + 0.00499999, 2))), '0') PremiumInPercentOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT - res2.MinProducerPrice)), '0')                                      PremiumInRubOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePrice)), '0')                                                                        PurchasePrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT)), '0')                                                                RetailPriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((((res2.RetailPriceWithoutVAT - res2.PurchasePrice) * 100) /
                                       res2.MinProducerPrice) + 0.00499999, 2))), '0')                                                              PremiumInPercentRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT - res2.PurchasePrice)), '0')                                           PremiumInRubRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxRetailPercent)), '0')                                                                     MaxRetailPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPrice)), '0')                                                                          RetailPrice
from res2 order by Name;";

        public static readonly string ForCsv1 = @"USE tempdb
declare @sqlQuery nvarchar(1000);
declare @ParmDefinition nvarchar(500);
set @ParmDefinition = N'@name nvarchar(1000), @barcode varchar(30)';
set @sqlQuery = 'select *
from tempdb.dbo.'+@tableName+' bal
where bal.Mhh + bal.Name + bal.Pro + bal.Series COLLATE SQL_Latin1_General_CP1251_CI_AS like @name
    and bal.Barcode + bal.Series like @barcode
ORDER by bal.Name'
EXEC sp_executesql @sqlQuery, @ParmDefinition, @name = @name, @barcode = @barcode;";

        public static readonly string ForCreateTempDataTable = @"USE tempdb
                        IF OBJECT_ID (N'temp_gd_stockbalance', N'U') IS NOT NULL
                            Drop Table temp_gd_stockbalance;
with res as (select apt.*, reg.O50_B, reg.O500_B, reg.O501_B, reg.R50_B, reg.R500_B, reg.R501_B
             from opeka_base.dbo.AptInfo apt
                    left join [vm-sql2008].[Ges].[dbo].[spr_region] reg on case
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
             where apt.idapt = @aptekaId),
     res0 as (SELECT ROW_NUMBER() over (partition by ean13, cen order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     r as (select * from res0 where id = 1),
     res0barcode as (SELECT ROW_NUMBER() over (partition by ean13 order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rbarcode as (select * from res0barcode where id = 1),
     res0IdposIdpro as (SELECT ROW_NUMBER() over (partition by idpos, idpro order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdposIdpro as (select * from res0IdposIdpro where id = 1),
     res0Idpos as (SELECT ROW_NUMBER() over (partition by idpos order by data_pril desc) id, * FROM [vm-sql2008].[Ges].[dbo].[Reestrreg] where region = @regionId),
     rIdpos as (select * from res0Idpos where id = 1),
     res1 as (select bal.IDApt                                                      IDApt,
                     apt.TaxType                                                    TaxType,
                     apt.O50_B                                                      O50_B,
                     apt.O500_B                                                     O500_B,
                     apt.O501_B                                                     O501_B,
                     apt.R50_B                                                      R50_B,
                     apt.R500_B                                                     R500_B,
                     apt.R501_B                                                     R501_B,
                     r.region                                                       region,
                     cl.naimen                                                      FullAptekaName,
                     coalesce(r.int, rbarcode.int, rIdposIdpro.int, rIdpos.int)     Mhh,
                     coalesce(r.name, rbarcode.name, rIdposIdpro.name, rIdpos.name) name,
                     coalesce(r.opis, rbarcode.opis, rIdposIdpro.opis, rIdpos.opis) opis,
                     coalesce(r.pro, rbarcode.pro, rIdposIdpro.pro, rIdpos.pro)     Pro,
                     bal.BarCode                                                    Barcode,
                     bal.VAT                                                        Nds,
                     bal.ProducerRegisteredPrice                                    ProducerRegisteredPrice,
                     bal.ProducerRealPrice                                          ProducerRealPrice,
                     bal.PurchasePriceWithoutVAT                                    PurchasePriceWithoutVAT,
                     bal.PurchasePrice                                              PurchasePrice,
                     case
                       when apt.TaxType = 'ТСН' then ROUND(bal.Price * 100 / (100 + bal.VAT) + 0.00499999, 2)
                       else bal.Price end                                           RetailPriceWithoutVAT,
                     bal.Price                                                      RetailPrice,
                     opeka_base.dbo.fnMathMin(case
                                                when bal.ProducerRegisteredPrice = 0 then bal.ProducerRealPrice
                                                else bal.ProducerRegisteredPrice end,
                                              case
                                                when bal.ProducerRealPrice = 0 then bal.ProducerRegisteredPrice
                                                else bal.ProducerRealPrice end)     MinProducerPrice
              from opeka_base.dbo.GD_StockBalance bal
                     left join r on bal.BarCode = r.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS and
                                    bal.ProducerRegisteredPrice = r.cen
                     left join rbarcode on bal.BarCode = rbarcode.ean13 COLLATE SQL_Latin1_General_CP1251_CI_AS
                     left join rIdposIdpro on bal.IDPos = rIdposIdpro.idpos and bal.IDPro = rIdposIdpro.idpro
                     left join rIdpos on bal.IDPos = rIdpos.idpos
                     left join res apt on apt.idapt = bal.IDApt
                     left join [Servsql].admzakaz.dbo.clients cl on cl.kp = bal.IDApt
              where bal.IDApt = @aptekaId
                and bal.IsLive = 1 and not coalesce(bal.properties,'') like '%карантин%'
                and not coalesce(bal.Properties, '') like '%apteka.ru%' and not coalesce(bal.Properties, '') like '%ZS%'
                and bal.Quantity > bal.Reserved
    ),
     res2 as (select case
                       when res1.MinProducerPrice <= 50 then res1.O50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.O500_B
                       when res1.MinProducerPrice > 500 then res1.O501_B
                         end MaxOptPercent,
                     case
                       when res1.MinProducerPrice <= 50 then res1.R50_B
                       when res1.MinProducerPrice > 50 and res1.MinProducerPrice <= 500 then res1.R500_B
                       when res1.MinProducerPrice > 500 then res1.R501_B
                         end MaxRetailPercent,
                     res1.*
              from res1)
select isnull(res2.FullAptekaName, '')                                                                                                              FullAptekaName,
       isnull(res2.Mhh, '')                                                                                                                         Mhh,
       isnull(res2.name, '') + ' ' + isnull(res2.opis, '')                                                                                          Name,
       isnull(res2.Pro, '')                                                                                                                         Pro,
       isnull(res2.BarCode, '')                                                                                                                     Barcode,
       isnull(CONVERT(VARCHAR(3), CONVERT(float, res2.Nds)), '0')                                                                                   Nds,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRegisteredPrice)), '0')                                                              ProducerRegisteredPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.ProducerRealPrice)), '0')                                                                    ProducerRealPrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxOptPercent)), '0')                                                                        MaxOptPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT)), '0')                                                              PurchasePriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((res2.PurchasePriceWithoutVAT - res2.MinProducerPrice) * 100 / res2.MinProducerPrice + 0.00499999, 2))), '0') PremiumInPercentOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePriceWithoutVAT - res2.MinProducerPrice)), '0')                                      PremiumInRubOpt,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.PurchasePrice)), '0')                                                                        PurchasePrice,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT)), '0')                                                                RetailPriceWithoutVAT,
       isnull(
         CONVERT(VARCHAR(50),
                 CONVERT(float, ROUND((((res2.RetailPriceWithoutVAT - res2.PurchasePrice) * 100) /
                                       res2.MinProducerPrice) + 0.00499999, 2))), '0')                                                              PremiumInPercentRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPriceWithoutVAT - res2.PurchasePrice)), '0')                                           PremiumInRubRetail,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.MaxRetailPercent)), '0')                                                                     MaxRetailPercent,
       isnull(CONVERT(VARCHAR(50), CONVERT(float, res2.RetailPrice)), '0')                                                                          RetailPrice
into temp_gd_stockbalance
from res2 order by Name;";

        public static readonly string ForCreateTempDataTable1 = @"execute opeka_base.dbo.spGetAptekaGNVLS @tableName, @aptekaId, @regionId;";

    }
}
