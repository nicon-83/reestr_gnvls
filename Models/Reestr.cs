using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace ReestrGNVLS.Models
{
    public partial class Reestr
    {
        [DisplayName("������")]
        [HiddenInput(DisplayValue = false)]
        public string FullAptekaName { get; set; }

        [DisplayName("�����")]
        public string Name { get; set; }

        [DisplayName("�������������")]
        public string Pro { get; set; }

        [DisplayName("MHH")]
        public string Mhh { get; set; }

        [DisplayName("�����")]
        public string Series { get; set; }

        [DisplayName("��������")]
        public string Barcode { get; set; }

        [DisplayName("���_%")]
        public string Nds { get; set; }

        [DisplayName("������������������ ���������� ��������� ���� �������������")]
        public string ProducerRegisteredPrice { get; set; }

        [DisplayName("����������� ��������� ����, ������������� ��������������, ��� ��� (������)")]
        public string ProducerRealPrice { get; set; }

        [DisplayName("����������� ��������� ����, ������������� ������������ ������� ��������, ��� ��� (������)")]
        public string PurchasePriceWithoutVAT { get; set; }

        [DisplayName("��������� ������ ����������� ������� ��������, ������������� ������������� ������� ��������, � ��������� %")]
        public string PremiumInPercentOpt { get; set; }

        [DisplayName("��������� ������ ����������� ������� ��������, ������������� ������������� ������� ��������, � ������")]
        public string PremiumInRubOpt { get; set; }

        [DisplayName("����������� ���������� ������� ������� ������� �� �������")]
        public string MaxOptPercent { get; set; }

        [DisplayName("����������� ��������� ����, ������������� ������������ ������� ��������, � ��� (������)")]
        public string PurchasePrice { get; set; }

        [DisplayName("����������� ��������� ����, ������������� ������������ ��������� ��������, ��� ��� (������)")]
        public string RetailPriceWithoutVAT { get; set; }

        [DisplayName("������ ����������� ��������� ��������, ������������� ������������ ��������� ��������, � ���������")]
        public string PremiumInPercentRetail { get; set; }

        [DisplayName("������ ����������� ��������� ��������, ������������� ������������ ��������� ��������, � ������")]
        public string PremiumInRubRetail { get; set; }

        [DisplayName("����������� ���������� ������� ��������� ������� �� �������")]
        public string MaxRetailPercent { get; set; }

        [DisplayName("����������� ��������� ����, ������������� ������������ ��������� ��������, � ��� (������)")]
        public string RetailPrice { get; set; }
    }

    public sealed class ReestrClassMap : ClassMap<Reestr>
    {
        public ReestrClassMap()
        {
            Map(m => m.FullAptekaName).Ignore();
            Map(m => m.Name).Name("�����");
            Map(m => m.Pro).Name("�������������");
            Map(m => m.Mhh).Name("MHH");
            Map(m => m.Series).Name("�����");
            Map(m => m.Barcode).Name("��������");
            Map(m => m.Nds).Name("��� %");
            Map(m => m.ProducerRegisteredPrice).Name("������������������ ���������� ��������� ���� �������������");
            Map(m => m.ProducerRealPrice).Name("����������� ��������� ����, ������������� ��������������, ��� ��� (������)");
            Map(m => m.PurchasePriceWithoutVAT).Name("����������� ��������� ����, ������������� ������������ ������� ��������, ��� ��� (������)");
            Map(m => m.PremiumInPercentOpt).Name("��������� ������ ����������� ������� ��������, ������������� ������������� ������� ��������, � ��������� %");
            Map(m => m.PremiumInRubOpt).Name("��������� ������ ����������� ������� ��������, ������������� ������������� ������� ��������, � ������");
            Map(m => m.MaxOptPercent).Name("����������� ���������� ������� ������� ������� �� ������� %");
            Map(m => m.PurchasePrice).Name("����������� ��������� ����, ������������� ������������ ������� ��������, � ��� (������)");
            Map(m => m.RetailPriceWithoutVAT).Name("����������� ��������� ����, ������������� ������������ ��������� ��������, ��� ��� (������)");
            Map(m => m.PremiumInPercentRetail).Name("������ ����������� ��������� ��������, ������������� ������������ ��������� ��������, � ��������� %");
            Map(m => m.PremiumInRubRetail).Name("������ ����������� ��������� ��������, ������������� ������������ ��������� ��������, � ������");
            Map(m => m.MaxRetailPercent).Name("����������� ���������� ������� ��������� ������� �� ������� %");
            Map(m => m.RetailPrice).Name("����������� ��������� ����, ������������� ������������ ��������� ��������, c ��� (������)");
        }
    }
}
