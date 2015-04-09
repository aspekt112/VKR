using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libsimdch;
using System.Threading;
using System.Globalization;


namespace simul
{
    public partial class Main : Form
    {
        private string strline = string.Empty;
        private DataTable dtresult;
        private DataTable dtStat;
        private DataTable dtStat1;
        private int ProgressVal;
        private string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public Main()
        {
            InitializeComponent();
            this.Text = global::simul.Properties.Resources.MainTitle;
            TMIParamVisible();
            DKParamVisible();
            EstParamVisible();


            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart1.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart1.Series["Series1"].Color = Color.Blue;
            chart1.Series["Series1"].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart1.Titles.Add("Состояние канала");

            chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart2.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart2.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            chart2.Series["Series1"].Color = Color.Green;
            chart2.Series["Series1"].IsVisibleInLegend = false;
            chart2.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart2.Titles.Add("Результат оценки состояния");


            chart3.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart3.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart3.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart3.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
            chart3.Series["Series1"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            chart3.Series["Series1"].IsVisibleInLegend = false;
            chart3.Series["Series1"].Color = Color.Red;
            chart3.Series["Series2"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            chart3.Series["Series2"].IsVisibleInLegend = false;
            chart3.Series["Series2"].Color = Color.Green;
            chart3.Series["Series3"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            chart3.Series["Series3"].IsVisibleInLegend = false;
            chart3.Series["Series3"].Color = Color.Blue;
            chart3.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart3.Titles.Add("Отображение канала связи");

        }

        private void speStateQw_ValueChanged(object sender, EventArgs e)
        {
            EstParamVisible();
        }


        // процедура обработки исходных данных
        private void InitDataTMI(ref string TMIType, ref string Helpline, ref string ParamLine)
        {
            
            if (rBRandom.Checked)
                TMIType = "Random";
            if (rBParity.Checked)
                TMIType = "Parity";
            if (rBDoubleParity.Checked)
                TMIType = "DoubleParity";
            // выводит вводимые параметры ТМИ в одну строку
            Helpline  = "TMILong=" + edbTMILong.Text + ";";
            Helpline = Helpline+ "WordLong="+ edbWordLong.Text+";";
            Helpline = Helpline+"BlockLong="+ edbBlockLong.Text+";";
            Helpline = Helpline+"WordParity="+ cbxParity.Text+";";

             ParamLine = "SumDg=0;";
            // оператор цикла, который выводит в строку paramline след:
            //"sumDg=0;sumbl1=0;,sumDg=0;sumbl=0;subl2=0; и т.д." - обнуляет для каждого числа битов

            for (int Cnt = 0; Cnt <= Convert.ToInt32(edbWordLong.Text)-1 ; Cnt++)
            {
                 ParamLine=ParamLine+"SumBl"+Convert.ToString(Cnt)+"=0;";
            }

            if (chxTMI.Checked) // если нажать кнопку "выводить поток" то
            {
               
                sgrResult.Columns.Clear();
                dtresult = new DataTable();


                int ColCount = Convert.ToInt32(functions.ReadParam("WordLong", Helpline)) + 6;
                int RowCount = (Convert.ToInt32(functions.ReadParam("TMILong", Helpline)) /
                                Convert.ToInt32(functions.ReadParam("WordLong", Helpline)))+1 ;

                // цикл определяет шапку таблицы
                for (int Cnt = 0; Cnt <= Convert.ToInt32(functions.ReadParam("WordLong", Helpline)) - 2; Cnt++)
                {
                 
                    DataColumn cl = new DataColumn("И"+Cnt, typeof(string));
                    cl.Caption = "И";
                    dtresult.Columns.Add(cl);
               
                }


                DataColumn clk = new DataColumn("К", typeof(string));
                clk.Caption = "К";
                dtresult.Columns.Add(clk);
                DataColumn clerr = new DataColumn("Ошибка", typeof(string));
                clerr.Caption = "Ошибка";
                dtresult.Columns.Add(clerr);
                DataColumn clst = new DataColumn("Сост", typeof(string));
                clst.Caption = "Сост";
                dtresult.Columns.Add(clst);
                DataColumn clpr = new DataColumn("Верош", typeof(string));
                clpr.Caption = "Верош";
                dtresult.Columns.Add(clpr);
                DataColumn clo = new DataColumn("Оценка", typeof(string));
                clo.Caption = "Оценка";
                dtresult.Columns.Add(clo);
                DataColumn cl1 = new DataColumn("col1", typeof(string));
                cl1.Caption = "";
                dtresult.Columns.Add(cl1);
                DataColumn cl2 = new DataColumn("col2", typeof(string));
                cl2.Caption = "";
                dtresult.Columns.Add(cl2);

                // Добавим строки 
                object[] row = new object[ColCount];
                for (int i = 0;i < ColCount; i++)
                {
                    row[i] = "";
                }


                for (int i = 0; i < RowCount; i++)
                {

                    dtresult.Rows.Add(row);
                }

                




                sgrResult.DataSource = dtresult;
                foreach (DataGridViewColumn col in sgrResult.Columns)
                {
                    col.HeaderText = dtresult.Columns[col.HeaderText].Caption;
                    if (col.HeaderText == "И")
                        col.Width = 25;
                    if (col.HeaderText == "К")
                        col.Width = 25;
                    if (col.HeaderText == "Ошибка")
                        col.Width = 50;
                    if (col.HeaderText == "Сост")
                        col.Width = 40;
                    if (col.HeaderText == "Верош")
                        col.Width = 40;
                    if (col.HeaderText == "Оценка")
                        col.Width = 50;
                    if (col.HeaderText == "")
                        col.Width = 40;
                }

                


                
            }

          


      }


        // выбор модели канала
        private void InitDataDK(ref string ChanModel, ref string HelplineDK, ref string ParamLineDK)
        {
           
            if (rBWithOutMemory.Checked)
                ChanModel = "WithOutMemory";
            if (rBGilbert.Checked)
                ChanModel = "Gilbert";
            if (rBEliotGilbert.Checked)
                ChanModel = "EliotGilbert";
            if (rBBenetFroilih.Checked)
                ChanModel = "BenetFroilih";

             HelplineDK = "PrCrGB="+edbPrCr1.Text+";";
             HelplineDK = HelplineDK+"PrCrBG="+edbPrCr2.Text+";";
             HelplineDK = HelplineDK+"PrErB="+edbPrEr1.Text+";";
             HelplineDK = HelplineDK+"PrErG="+edbPrEr2.Text+";";

            // получим пример:
            // PrCrGB=0.01;PrCrBG=;PrErB=10;PrErG=;
            ParamLineDK = "PrEr="+ edbPrEr1.Text+";"; // вер-ть ош в плх сост
            ParamLineDK = "St=0;"+ParamLineDK;

        }

        // выбор критерия оценки
        private void InitEstimate(ref string EstLine)
        {
             // выдает колво состояний StateQw=1(2);
            EstLine="StateQw="+Convert.ToString(speStateQw.Value)+";";
            if (rBNaiman.Checked)
                EstLine=EstLine+"CrType=Naiman;";
            if (rBVald.Checked)
                EstLine=EstLine+"CrType=Vald;";
            // вывод данных по оценке в одну строку
             EstLine=EstLine+"Border12="+edbBorder12.Text+";";
             EstLine=EstLine+"Border23="+edbBorder23.Text+";";
             EstLine=EstLine+"SelLong12="+edbSelLong12.Text+";";
             EstLine=EstLine+"SelLong23="+edbSelLong23.Text+";";
             EstLine=EstLine+"Shield12="+edbShield12.Text+";";
             EstLine=EstLine+"Shield23="+edbShield23.Text+";";
             EstLine=EstLine+"EstStart=1;";
             EstLine=EstLine+"EstErSum=0;";
             EstLine=EstLine+"EstDgSum=0;";
            // получим пример:
            // StateQw=1;CrType=Naiman;Border12=10;...EstStart=1;EstDgSum=0;
    
        }


        // процедура визуализации параметров генерации тми
        private void TMIParamVisible()
        {
            if (rBRandom.Checked)
            {
                lblWordLong.Visible=false;  // если выбрать "случ" то
                edbWordLong.Visible=false;  // исчезает дл слова, дл блока,
                lblBlockLong.Visible=false; // контроль по (не)ч
                edbBlockLong.Visible=false;
                lblParity.Visible=false;
                cbxParity.Visible = false;

            }
            if (rBParity.Checked)
            {
                // исчезает дл блока
                lblWordLong.Visible = true;
                edbWordLong.Visible = true;
                lblBlockLong.Visible=false;
                edbBlockLong.Visible=false;
                lblParity.Visible = true;
                cbxParity.Visible = true;
            }
            if (rBDoubleParity.Checked)
            {
                lblWordLong.Visible = true;
                edbWordLong.Visible = true;
                lblBlockLong.Visible = true;
                edbBlockLong.Visible = true;
                lblParity.Visible = true;
                cbxParity.Visible = true;

            }
             
        }


        // проц визуализации параметров канала
        private void DKParamVisible()
        {
            if (rBWithOutMemory.Checked)
            {
                 lblPrCr1.Visible=false;  
                 edbPrCr1.Visible=false;
                 lblPrCr2.Visible=false;  
                 edbPrCr2.Visible=false;
                 lblPrEr1.Text="Вероятность ошибки";
                 lblPrEr1.Visible=true;
                 edbPrEr1.Visible=true;
                 lblPrEr2.Visible=false;  
                 edbPrEr2.Visible=false;
            
            }
            if (rBGilbert.Checked)
            {
                lblPrCr1.Text="Вероятность перехода в плохое состояние -";
                lblPrCr1.Visible=true;   edbPrCr1.Visible=true;
                lblPrCr2.Text="Вероятность перехода в хорошее состояние -";
                lblPrCr2.Visible=true;   edbPrCr2.Visible=true;
                lblPrEr1.Text="Вероятность ошибки в плохом состоянии -";
                lblPrEr1.Visible=true;   edbPrEr1.Visible=true;
                edbPrEr2.Text="0";
                lblPrEr2.Visible=false;  edbPrEr2.Visible=false;

            }
            if (rBEliotGilbert.Checked)
            {
                 lblPrCr1.Text="Вероятность перехода в плохое состояние -";
                 lblPrCr1.Visible=true;   edbPrCr1.Visible=true;
                 lblPrCr2.Text="Вероятность перехода в хорошее состояние -";
                 lblPrCr2.Visible=true;   edbPrCr2.Visible=true;
                 lblPrEr1.Text="Вероятность ошибки в хорошем состоянии -";
                 lblPrEr1.Visible=true;   edbPrEr1.Visible=true;
                 lblPrEr1.Text="Вероятность ошибки в плохом состоянии -";
                 lblPrEr2.Visible=true;   edbPrEr2.Visible=true;

            }
            if (rBBenetFroilih.Checked)
            {
                 lblPrCr1.Text="Вероятность начала пакета -";
                 lblPrCr1.Visible=true;   edbPrCr1.Visible=true;
                 lblPrCr2.Text="Вероятность конца пакета -";
                 lblPrCr2.Visible=true;   edbPrCr2.Visible=true;
                 lblPrEr1.Text="Вероятность ошибки в пакете -";
                 lblPrEr1.Visible=true;   edbPrEr1.Visible=true;
                 lblPrEr2.Visible=false;  edbPrEr2.Visible=false;

            }


        }



        private void InitStatTables(int var)
        {
            dtStat = new DataTable();
            dtStat1 = new DataTable();

            sgrStat.Columns.Clear();
            sgrStat1.Columns.Clear();

            switch (var)
            {
                case 2:
                    // шапка таблицы
                     DataColumn cl0 = new DataColumn("ЗАГ", typeof(string));
                    DataColumn cl00 = new DataColumn("ЗАГ", typeof(string));
                    cl0.Caption = "";
                    cl00.Caption = "";
                    dtStat.Columns.Add(cl0);
                    dtStat1.Columns.Add(cl00);


                    DataColumn cl1 = new DataColumn("ХОР", typeof(string));
                    DataColumn cl11 = new DataColumn("ХОР", typeof(string));
                    cl1.Caption = "ХОР";
                    cl11.Caption = "ХОР";
                    dtStat.Columns.Add(cl1);
                    dtStat1.Columns.Add(cl11);
                    DataColumn cl2 = new DataColumn("ПЛХ", typeof(string));
                    DataColumn cl22 = new DataColumn("ПЛХ", typeof(string));
                    cl2.Caption = "ПЛХ";
                    cl22.Caption = "ПЛХ";
                    dtStat.Columns.Add(cl2);
                    dtStat1.Columns.Add(cl22);
                    DataColumn cl3 = new DataColumn("СУМ", typeof(string));
                    DataColumn cl33 = new DataColumn("СУМ", typeof(string));
                    cl3.Caption = "СУМ";
                    cl33.Caption = "СУМ";
                    dtStat.Columns.Add(cl3);
                    dtStat1.Columns.Add(cl33);

                    // Добавим строки 
                    object[] row = new object[3];
                    object[] row1 = new object[3];
                    for (int i = 0; i < 3; i++)
                    {
                        row[i] = "";
                        row1[i] = "";
                       
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        dtStat.Rows.Add(row);
                        dtStat1.Rows.Add(row1);
                    }

                    sgrStat.DataSource = dtStat;
                    sgrStat1.DataSource = dtStat1;

                    foreach (DataGridViewColumn col in sgrStat.Columns)
                     {
                         col.HeaderText = dtStat.Columns[col.HeaderText].Caption;
                         col.Width = 50;
                   
                     }
                    foreach (DataGridViewColumn col in sgrStat1.Columns)
                    {
                        col.HeaderText = dtStat1.Columns[col.HeaderText].Caption;
                        col.Width = 50;

                    }

                    dtStat.Rows[0][0] = "ХОР";
                    dtStat.Rows[1][0] = "ПЛХ";
                    dtStat.Rows[2][0] = "СУМ";
                    dtStat1.Rows[0][0] = "ХОР";
                    dtStat1.Rows[1][0] = "ПЛХ";
                    dtStat1.Rows[2][0] = "СУМ";

                    DataGridViewCellStyle st = sgrStat.Columns[0].DefaultCellStyle;
                    st.BackColor = SystemColors.Control;
                    sgrStat.Columns[0].Frozen = true;
                    DataGridViewCellStyle st1 = sgrStat1.Columns[0].DefaultCellStyle;
                    st1.BackColor = SystemColors.Control;
                    sgrStat1.Columns[0].Frozen = true;


                    break;
                case 3:
                    // шапка таблицы
                    DataColumn clx0 = new DataColumn("ЗАГ", typeof(string));
                    DataColumn clx00 = new DataColumn("ЗАГ", typeof(string));
                    clx0.Caption = "";
                    clx00.Caption = "";
                    dtStat.Columns.Add(clx0);
                    dtStat1.Columns.Add(clx00);
                    DataColumn clx1 = new DataColumn("ХОР", typeof(string));
                    DataColumn clx11 = new DataColumn("ХОР", typeof(string));
                    clx1.Caption = "ХОР";
                    clx11.Caption = "ХОР";
                    dtStat.Columns.Add(clx1);
                    dtStat1.Columns.Add(clx11);
                    DataColumn clx2 = new DataColumn("УД", typeof(string));
                    DataColumn clx22 = new DataColumn("УД", typeof(string));
                    clx2.Caption = "УД";
                    clx22.Caption = "УД";
                    dtStat.Columns.Add(clx2);
                    dtStat1.Columns.Add(clx22);
                    DataColumn clx3 = new DataColumn("ПЛХ", typeof(string));
                    DataColumn clx33 = new DataColumn("ПЛХ", typeof(string));
                    clx3.Caption = "ПЛХ";
                    clx33.Caption = "ПЛХ";
                    dtStat.Columns.Add(clx3);
                    dtStat1.Columns.Add(clx33);
                    DataColumn clx4 = new DataColumn("СУМ", typeof(string));
                    DataColumn clx44 = new DataColumn("СУМ", typeof(string));
                    clx4.Caption = "СУМ";
                    clx44.Caption = "СУМ";
                    dtStat.Columns.Add(clx4);
                    dtStat1.Columns.Add(clx44);

                    object[] rowx1 = new object[4];
                    object[] rowx11 = new object[4];
                    for (int i = 0; i < 4; i++)
                    {
                        rowx1[i] = "";
                        rowx11[i] = "";
                       
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        dtStat.Rows.Add(rowx1);
                        dtStat1.Rows.Add(rowx11);
                    }

                    sgrStat.DataSource = dtStat;
                    sgrStat1.DataSource = dtStat1;

                    foreach (DataGridViewColumn col in sgrStat.Columns)
                     {
                         col.HeaderText = dtStat.Columns[col.HeaderText].Caption;
                         col.Width = 50;
                   
                     }
                    foreach (DataGridViewColumn col in sgrStat1.Columns)
                    {
                        col.HeaderText = dtStat1.Columns[col.HeaderText].Caption;
                        col.Width = 50;

                    }

                     dtStat.Rows[0][0] = "ХОР";
                     dtStat.Rows[1][0] = "УД";   
                     dtStat.Rows[2][0] = "ПЛХ";
                     dtStat.Rows[3][0] = "СУМ";
                     dtStat1.Rows[0][0] = "ХОР";
                     dtStat1.Rows[1][0] = "УД";   
                     dtStat1.Rows[2][0] = "ПЛХ";
                     dtStat1.Rows[3][0] = "СУМ";


                    DataGridViewCellStyle stx = sgrStat.Columns[0].DefaultCellStyle;
                    stx.BackColor = SystemColors.Control;
                    sgrStat.Columns[0].Frozen = true;
                    DataGridViewCellStyle stx1 = sgrStat1.Columns[0].DefaultCellStyle;
                    stx1.BackColor = SystemColors.Control;
                    sgrStat1.Columns[0].Frozen = true;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
           
          

        }







        // процедура визуализации параметров оценки
        private void EstParamVisible()
        {
            switch ((int)speStateQw.Value)
            {
                case 2:
                    edbBorder23.Visible = false;
                    edbSelLong23.Visible = false;
                    edbShield23.Visible = false;
                    lblBorder1234.Text = "Плохой / Хороший";

                    InitStatTables(2);

                    break;

                case 3:
                    edbBorder23.Visible = true;
                    if (rBNaiman.Checked)
                        edbSelLong23.Visible = false;
                    if (rBVald.Checked)
                        edbSelLong23.Visible = true;

                    edbShield23.Visible = true;
                    lblBorder1234.Text = "Плохой / Удов.  / Хороший";

                    InitStatTables(3);
                   
                    break;
                default:
                    throw new ArgumentOutOfRangeException("speStateQw");
            }
        }




        private void MainAlgoritm()
        {
            int DigitalCount; int TMILong; int WordLong;
            int Sum0; int Sum1; int SumEr;
            byte CurrentDigital; byte EstimateDigital;
            string TMIType = string.Empty; string HelpLine = string.Empty; string ParamLine = string.Empty;
            string ChanModel = string.Empty; string HelpLineDK = string.Empty; string ParamLineDK = string.Empty;
            string EstLine = string.Empty;
            bool StatStart;


            // создаем строку исходных данных генерации
            InitDataTMI(ref TMIType, ref HelpLine, ref ParamLine);
            InitDataDK(ref ChanModel, ref HelpLineDK, ref ParamLineDK);
            InitEstimate(ref EstLine);

            TMILong = Convert.ToInt32(edbTMILong.Text);// ввод числа задаваемых бит
            //pbrTime.Max:=TMILong; // днительность синей строки расчета= длительности бит
            Sum0 = 0;    // сумма нулей
            Sum1 = 0;    // сумма единиц
            SumEr = 0;   // сумма ошибок
            lblTimeB.Text = "Время начала    :" + DateTime.Now.ToLongTimeString();// вывод времени начала
            DigitalCount = 1; // присваивание цифрового значения=1

            // генерируем случ число по выбран закону
            CurrentDigital = functions.TMIGeneration(TMIType, DigitalCount, HelpLine, ref ParamLine);
            InitDataTMI(ref TMIType, ref HelpLine, ref ParamLine);







            StatStart = false;

            ProgressVal = 0;
           
            // пока цифра  <= заданному числу бит то
            while (DigitalCount <= TMILong)
            {
                // генерирум случ числа
                CurrentDigital = functions.TMIGeneration(TMIType, DigitalCount, HelpLine, ref ParamLine);
                // выбираем модель канала передачи
                EstimateDigital = functions.ChannelTransmit(ChanModel, CurrentDigital, HelpLineDK, ref ParamLineDK);


                if (StatStart)
                    functions.ChannelEstimate(EstimateDigital, TMIType, DigitalCount, HelpLine, ref EstLine);

                //if (DigitalCount%(TMILong/100)==0) 
                // pbrTime.Position:=DigitalCount;

                if (StatStart)
                {
                    if (CurrentDigital == 1)
                        Sum1++;
                    else
                        Sum0++;
                    WordLong = Convert.ToInt32(functions.ReadParam("WordLong", HelpLine));

                    // еcли нажать кнопку выводить поток то заполняет таблицу

                    if (chxTMI.Checked)
                        OutTMIInTable(DigitalCount, WordLong, CurrentDigital, EstimateDigital, ParamLineDK, EstLine);


                    // если случ число не равно оцененому по модели то подсчитывает колво ошибок
                    if (CurrentDigital != EstimateDigital)
                        SumEr++;
                }
                DigitalCount++;
                ProgressVal++;
                progressBar1.Value = progressBar1.Maximum * ProgressVal / TMILong;
                if (DigitalCount > (TMILong / 10) && !StatStart)
                {
                    DigitalCount = 1;
                    ProgressVal = 1;
                    StatStart = true;
                    InitDataTMI(ref TMIType, ref HelpLine, ref ParamLine);
                    InitEstimate(ref EstLine);
                   
                }


            }

            lblTimeE.Text = "Время окончания :" + DateTime.Now.ToLongTimeString();
            lblSum0.Text = "Количество нулей    = " + Sum0.ToString();
            lblSum1.Text = "Количество единиц  = " + Sum1.ToString();
            lblSumEr.Text = "Количество ошибок = " + SumEr.ToString();
            EstHelp(HelpLine);

        }









        // нажатие кнопки начать моделирование
        private void BtnSimulStart_Click(object sender, EventArgs e)
        {
                MainAlgoritm();
      
        }

        private void rBWithOutMemory_CheckedChanged(object sender, EventArgs e)
        {
            DKParamVisible();
        }

        private void rBGilbert_CheckedChanged(object sender, EventArgs e)
        {
            DKParamVisible();
        }

        private void rBEliotGilbert_CheckedChanged(object sender, EventArgs e)
        {
            DKParamVisible();
        }

        private void rBBenetFroilih_CheckedChanged(object sender, EventArgs e)
        {
            DKParamVisible();
        }

        private void rBRandom_CheckedChanged(object sender, EventArgs e)
        {
            TMIParamVisible();
        }

        private void rBParity_CheckedChanged(object sender, EventArgs e)
        {
            TMIParamVisible();
        }

        private void rBDoubleParity_CheckedChanged(object sender, EventArgs e)
        {
            TMIParamVisible();
        }

        private void rBNaiman_CheckedChanged(object sender, EventArgs e)
        {
            EstParamVisible();
        }

        private void rBVald_CheckedChanged(object sender, EventArgs e)
        {
            TMIParamVisible();
        }





        private void OutTMIInTable(int DigitalCount, int WordLong, byte CurrentDigital, byte EstimateDigital, string ParamLineDK, string EstLine)
        {

            int Cl=DigitalCount%WordLong-1;
            int Rw=DigitalCount/WordLong;

            if (Rw==1)
                dtresult.Rows[Rw][WordLong] = "";

            if (Cl==-1) 
            {
                 Cl=WordLong-1;   // значение строки
                 Rw=Rw-1;         // значение столбца

            }
            dtresult.Rows[Rw][Cl] = CurrentDigital.ToString();
     
            // если число не равно оцененному то ошибка
                if (CurrentDigital!=EstimateDigital)
                {
                    dtresult.Rows[Rw][WordLong] += "Ошибка"; 
                    // и записывается 1 и 0
                     dtresult.Rows[Rw][Cl]=CurrentDigital.ToString()+  EstimateDigital.ToString();
                }

                switch (functions.ReadParam("StateQw", EstLine))
                {
                    case "2":
                        // если вер ошибки больше границы плохого канала то запис плохой иначе хороший
                            if (Convert.ToDouble(functions.ReadParam("Border12",EstLine))<
                                Convert.ToDouble(functions.ReadParam("PrEr",ParamLineDK))) 
                                dtresult.Rows[Rw][WordLong+1] = "Плох";
                            else
                                 dtresult.Rows[Rw][WordLong+1] = "Хор";
                    
                        break;
                    case "3":
                        if (Convert.ToDouble(functions.ReadParam("Border12", EstLine)) <
                            Convert.ToDouble(functions.ReadParam("PrEr", ParamLineDK)))
                            dtresult.Rows[Rw][WordLong + 1] = "Плох";
                        else
                            if (Convert.ToDouble(functions.ReadParam("Border23", EstLine)) <
                               Convert.ToDouble(functions.ReadParam("PrEr", ParamLineDK)))
                              dtresult.Rows[Rw][WordLong + 1] = "Удов";
                            else
                              dtresult.Rows[Rw][WordLong + 1] = "Хор";

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("StateQw");

                }
                dtresult.Rows[Rw][WordLong + 2] = functions.ReadParam("PrEr",ParamLineDK);
                dtresult.Rows[Rw][WordLong + 3] = functions.ReadParam("StateEst", EstLine);
                if (functions.ReadParam("StateEst", EstLine) != "_")
                    dtresult.Rows[Rw][WordLong + 4] = "Est";
                    dtresult.Rows[Rw+1][WordLong + 4] = "";
                    dtresult.Rows[Rw][WordLong + 5] = DigitalCount.ToString();
                    dtresult.Rows[Rw + 1][WordLong] = "";


                  
          
        }

           // создание строки оценки
     private void EstHelp(string HelpLine)
    {
        int WordLong = Convert.ToInt32(edbWordLong.Text);
        double SumBB = 0; double SumBM = 0; double SumBG = 0;
        double SumMB = 0; double SumMM = 0; double SumMG = 0;
        double SumGB = 0; double SumGM = 0; double SumGG = 0;
        int Rw = dtresult.Rows.Count-1; //присваивает значение -1 ой строки
         while (Rw >= 0)
         {
           
             if ( dtresult.Rows[Rw][WordLong + 3].ToString() == "_")
             {
                 dtresult.Rows[Rw][WordLong + 3] = dtresult.Rows[Rw+1][WordLong + 3];
             }
             if (dtresult.Rows[Rw][WordLong + 3].ToString() != "")
             {
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Хор") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Плох"))
                    SumGB++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Хор") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Удов"))
                    SumGM++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Хор") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Хор"))
                     SumGG++;

                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Удов") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Плох"))
                     SumMB++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Удов") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Удов"))
                     SumMM++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Удов") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Хор"))
                     SumMG++;

                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Плох") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Плох"))
                     SumBB++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Плох") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Удов"))
                     SumBM++;
                 if ((dtresult.Rows[Rw][WordLong + 1].ToString() == "Плох") && (dtresult.Rows[Rw][WordLong + 3].ToString() == "Хор"))
                     SumBG++;

             }

           
             Rw--;
         }

         // заполнение таблицы оценок абсолютной
         switch ((int)speStateQw.Value)
         {
             case 2:
                 dtStat.Rows[0][1] = SumGG.ToString();
                 dtStat.Rows[0][2] = SumGB.ToString();
                 dtStat.Rows[1][1] = SumBG.ToString();
                 dtStat.Rows[1][2] = SumBB.ToString();
                 dtStat.Rows[2][1] = (SumGG+SumBG).ToString();
                 dtStat.Rows[2][2] = (SumGB+SumBB).ToString();
                 dtStat.Rows[0][3] = (SumGG+SumGB).ToString();
                 dtStat.Rows[1][3] = (SumBG+SumBB).ToString();

               

                 dtStat1.Rows[0][1] = ((SumGG/(SumGG+SumGB))*100).ToString();
                 dtStat1.Rows[0][2] = ((SumGB/(SumGG + SumGB)) * 100).ToString();
                 dtStat1.Rows[0][3] = (((SumGG + SumGB) / (SumGG +  SumGB + SumBG + SumBB)) * 100).ToString();

                 dtStat1.Rows[1][1] = ((SumBG/(SumBG+ SumBB))*100).ToString();
                 dtStat1.Rows[1][2] = ((SumBB/(SumBG + SumBB)) * 100).ToString();
                 dtStat1.Rows[1][3] = (((SumBG + SumBB) / (SumGG +  SumGB + SumBG + SumBB)) * 100).ToString();

                 dtStat1.Rows[2][1] = (((SumGG + SumBG) / (SumGG + SumGB + SumBG + SumBB)) * 100).ToString();
                 dtStat1.Rows[2][2] = (((SumGB + SumBB) / (SumGG + SumGB + SumBG + SumBB)) * 100).ToString();
                 
                  break;

             case 3:
                  dtStat.Rows[0][1] = SumGG.ToString();
                  dtStat.Rows[0][2] = SumGM.ToString();
                  dtStat.Rows[0][3] = SumGB.ToString();
                  dtStat.Rows[0][4] = (SumGB + SumGM + SumGB).ToString();
                  dtStat.Rows[1][1] = SumMG.ToString();
                  dtStat.Rows[1][2] = SumMM.ToString();
                  dtStat.Rows[1][3] = SumMB.ToString();
                  dtStat.Rows[1][4] = (SumMG + SumMM + SumMB).ToString();
                  dtStat.Rows[2][1] = SumBG.ToString();
                  dtStat.Rows[2][2] = SumBM.ToString();
                  dtStat.Rows[2][3] = SumBB.ToString();
                  dtStat.Rows[2][4] = (SumBG + SumBM + SumBB).ToString();
                  dtStat.Rows[3][1] = (SumGG + SumMG + SumBG).ToString();
                  dtStat.Rows[3][2] = (SumGM + SumMM + SumBM).ToString();
                  dtStat.Rows[3][3] = (SumGB + SumMB + SumBB).ToString();


                 dtStat1.Rows[0][1] = ((SumGG / (SumGG + SumGM + SumGB)) * 100).ToString();
                 dtStat1.Rows[0][2] = ((SumGM / (SumGG + SumGM + SumGB)) * 100).ToString();
                 dtStat1.Rows[0][3] = ((SumGB / (SumGG + SumGM + SumGB)) * 100).ToString();
                 dtStat1.Rows[0][4] = (((SumGG + SumGM + SumGB) / (SumGG + SumGM + SumGB + SumMG + SumMM + SumMB + SumBG + SumBM + SumBB)) * 100).ToString();

                 dtStat1.Rows[1][1] = ((SumMG/(SumMG+SumMM+SumMB))*100).ToString();
                 dtStat1.Rows[1][2] = ((SumMM/(SumMG+SumMM+SumMB)) * 100).ToString();
                 dtStat1.Rows[1][3] = ((SumMB/(SumMG+SumMM+SumMB)) * 100).ToString();
                 dtStat1.Rows[1][4] = (((SumMG + SumMM + SumMB) / (SumGG +  SumGB + SumBG + SumBB)) * 100).ToString();

                 dtStat1.Rows[2][1] = ((SumBG/(SumBG+SumBM+SumBB))*100).ToString();
                 dtStat1.Rows[2][2] = ((SumBM /(SumBG + SumBM + SumBB)) * 100).ToString();
                 dtStat1.Rows[2][3] = ((SumBB /(SumBG + SumBM + SumBB)) * 100).ToString();
                 dtStat1.Rows[2][4] = (((SumBG + SumBM + SumBB) / (SumGG + SumGB + SumBG + SumBB)) * 100).ToString();

                 dtStat1.Rows[3][1] = (((SumGG + SumMG + SumBG) / (SumGG + SumGM + SumGB + SumMG + SumMM + SumMB + SumBG + SumBM + SumBB)) * 100).ToString();
                 dtStat1.Rows[3][2] = (((SumGM + SumMM + SumBM) / (SumGG + SumGM + SumGB + SumMG + SumMM + SumMB + SumBG + SumBM + SumBB)) * 100).ToString();
                 dtStat1.Rows[3][3] = (((SumGB + SumMB + SumBB) / (SumGG + SumGM + SumGB + SumMG + SumMM + SumMB + SumBG + SumBM + SumBB)) * 100).ToString();


                 break;
             default:
                 throw new ArgumentOutOfRangeException("speStateQw");
         }

         // Верность
        lblTrue.Text="Верность= "+ (SumGG+SumMM+SumBB)/(SumGG+SumGM+SumGB+SumMG+SumMM+SumMB+SumBG+SumBM+SumBB);

      


    }

     private void btnChart_Click(object sender, EventArgs e)
     {
         chart1.Series["Series1"].Points.Clear();
         chart2.Series["Series1"].Points.Clear();
         chart3.Series["Series1"].Points.Clear();
         chart3.Series["Series2"].Points.Clear();
         chart3.Series["Series3"].Points.Clear();
         int WordLong = Convert.ToInt32(edbWordLong.Text);
         int Yval = 0;

         if (dtresult != null)
         {
             for (int i = 0; i < dtresult.Rows.Count; i++)
             {
                 if (dtresult.Rows[i][WordLong + 1].ToString() == "Плох")
                 {
                     Yval = 60;
                     chart1.Series["Series1"].Points.AddY(Yval);
                    
                 }
                 if (dtresult.Rows[i][WordLong + 1].ToString() == "Удов")
                 {
                     Yval = 30;
                     chart1.Series["Series1"].Points.AddY(Yval);
                 }
                 if (dtresult.Rows[i][WordLong + 1].ToString() == "Хор")
                 {
                     Yval = 5;
                     chart1.Series["Series1"].Points.AddY(Yval);
                 }
             }

             for (int i = 0; i < dtresult.Rows.Count; i++)
             {
                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Плох")
                 {
                     Yval = 60;
                     chart2.Series["Series1"].Points.AddY(Yval);

                 }
                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Удов")
                 {
                     Yval = 30;
                     chart2.Series["Series1"].Points.AddY(Yval);
                 }
                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Хор")
                 {
                     Yval = 5;
                     chart2.Series["Series1"].Points.AddY(Yval);
                 }
             }

             for (int i = 0; i < dtresult.Rows.Count; i++)
             {

                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Плох")
                 {
                     Yval = 40;
                     chart3.Series["Series1"].Points.AddY(Yval);
                 }
                 else
                 {
                     Yval = 0;
                     chart3.Series["Series1"].Points.AddY(Yval);
                 }

                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Удов")
                 {
                     Yval = 40;
                     chart3.Series["Series2"].Points.AddY(Yval);
                 }
                 else
                 {
                     Yval = 0;
                     chart3.Series["Series2"].Points.AddY(Yval);
                 }
                 if (dtresult.Rows[i][WordLong + 3].ToString() == "Хор")
                 {
                     Yval = 40;
                     chart3.Series["Series3"].Points.AddY(Yval);
                 }
                 else
                 {
                     Yval = 0;
                     chart3.Series["Series3"].Points.AddY(Yval);
                 }

               
             }


         }

         


     }

     private void btnClearChart_Click(object sender, EventArgs e)
     {
         chart1.Series["Series1"].Points.Clear();
         chart2.Series["Series1"].Points.Clear();
         chart3.Series["Series1"].Points.Clear();
     }

    

    

    
       

    

      



        
    }
}
