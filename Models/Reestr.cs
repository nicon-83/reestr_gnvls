using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace ReestrGNVLS.Models
{
    public partial class Reestr
    {
        [DisplayName("Аптека")]
        [HiddenInput(DisplayValue = false)]
        public string FullAptekaName { get; set; }

        [DisplayName("Товар")]
        public string Name { get; set; }

        [DisplayName("Производитель")]
        public string Pro { get; set; }

        [DisplayName("MHH")]
        public string Mhh { get; set; }

        [DisplayName("Серия")]
        public string Series { get; set; }

        [DisplayName("Штрихкод")]
        public string Barcode { get; set; }

        [DisplayName("НДС_%")]
        public string Nds { get; set; }

        [DisplayName("Зарегистрированная предельная отпускная цена производителя")]
        public string ProducerRegisteredPrice { get; set; }

        [DisplayName("Фактическая отпускная цена, установленная производителем, без НДС (рублей)")]
        public string ProducerRealPrice { get; set; }

        [DisplayName("Фактическая отпускная цена, установленная организацией оптовой торговли, без НДС (рублей)")]
        public string PurchasePriceWithoutVAT { get; set; }

        [DisplayName("Суммарный размер фактических оптовых надбавок, установленных организациями оптовой торговли, в процентах %")]
        public string PremiumInPercentOpt { get; set; }

        [DisplayName("Суммарный размер фактических оптовых надбавок, установленных организациями оптовой торговли, в рублях")]
        public string PremiumInRubOpt { get; set; }

        [DisplayName("Максимально допустимый процент оптовой наценки по региону")]
        public string MaxOptPercent { get; set; }

        [DisplayName("Фактическая отпускная цена, установленная организацией оптовой торговли, с НДС (рублей)")]
        public string PurchasePrice { get; set; }

        [DisplayName("Фактическая отпускная цена, установленная организацией розничной торговли, без НДС (рублей)")]
        public string RetailPriceWithoutVAT { get; set; }

        [DisplayName("Размер фактической розничной надбавки, установленной организацией розничной торговли, в процентах")]
        public string PremiumInPercentRetail { get; set; }

        [DisplayName("Размер фактической розничной надбавки, установленной организацией розничной торговли, в рублях")]
        public string PremiumInRubRetail { get; set; }

        [DisplayName("Максимально допустимый процент розничной наценки по региону")]
        public string MaxRetailPercent { get; set; }

        [DisplayName("Фактическая отпускная цена, установленная организацией розничной торговли, с НДС (рублей)")]
        public string RetailPrice { get; set; }
    }

    public sealed class ReestrClassMap : ClassMap<Reestr>
    {
        public ReestrClassMap()
        {
            Map(m => m.FullAptekaName).Ignore();
            Map(m => m.Name).Name("Товар");
            Map(m => m.Pro).Name("Производитель");
            Map(m => m.Mhh).Name("MHH");
            Map(m => m.Series).Name("Серия");
            Map(m => m.Barcode).Name("Штрихкод");
            Map(m => m.Nds).Name("НДС %");
            Map(m => m.ProducerRegisteredPrice).Name("Зарегистрированная предельная отпускная цена производителя");
            Map(m => m.ProducerRealPrice).Name("Фактическая отпускная цена, установленная производителем, без НДС (рублей)");
            Map(m => m.PurchasePriceWithoutVAT).Name("Фактическая отпускная цена, установленная организацией оптовой торговли, без НДС (рублей)");
            Map(m => m.PremiumInPercentOpt).Name("Суммарный размер фактических оптовых надбавок, установленных организациями оптовой торговли, в процентах %");
            Map(m => m.PremiumInRubOpt).Name("Суммарный размер фактических оптовых надбавок, установленных организациями оптовой торговли, в рублях");
            Map(m => m.MaxOptPercent).Name("Максимально допустимый процент оптовой наценки по региону %");
            Map(m => m.PurchasePrice).Name("Фактическая отпускная цена, установленная организацией оптовой торговли, с НДС (рублей)");
            Map(m => m.RetailPriceWithoutVAT).Name("Фактическая отпускная цена, установленная организацией розничной торговли, без НДС (рублей)");
            Map(m => m.PremiumInPercentRetail).Name("Размер фактической розничной надбавки, установленной организацией розничной торговли, в процентах %");
            Map(m => m.PremiumInRubRetail).Name("Размер фактической розничной надбавки, установленной организацией розничной торговли, в рублях");
            Map(m => m.MaxRetailPercent).Name("Максимально допустимый процент розничной наценки по региону %");
            Map(m => m.RetailPrice).Name("Фактическая отпускная цена, установленная организацией розничной торговли, c НДС (рублей)");
        }
    }
}
