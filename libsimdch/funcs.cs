using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libsimdch
{
    public static class functions
    {
        // Чтение параметра
       public static string ReadParam(string Param, string HelpLine)
        {
           
            string res;
            
            int ParamPos = HelpLine.IndexOf(Param, 0);
            int ParamValB = HelpLine.IndexOf("=", ParamPos);
            int ParamValE = HelpLine.IndexOf(";", ParamPos);
            res = HelpLine.Substring(ParamValB+1, ParamValE - ParamValB-1);
            return res;

        }


    public static void  ChangeParam( string Param, string Val, ref string HelpLine)
    {
        // парампос определяет положение парам в строке хэлплайн
        // и складывает с длиной подстроки плюс 1
        // например хэлплайн: 'длина слова=10;'-15 символов
        // парам - длина слова= -12 символов
        string res;

         if (!HelpLine.Contains(Param))
              res = HelpLine + Param + "=" + Val + ";";
         else
         {
              int ParamPos = HelpLine.IndexOf(Param, 0);
              int ParamPosEnd = HelpLine.IndexOf(";", ParamPos);
              string fulparam = HelpLine.Substring(ParamPos, ParamPosEnd - ParamPos+1);
              res = HelpLine.Replace(fulparam, Param + "=" + Val + ";");

         }

        HelpLine = res;


        

    }



        // функция возведения в степень exponent числа base
       public static double Power(double Base, double Exponent)
        {
            return Math.Exp(Exponent * Math.Log(Base,Math.E));
        }


      public  static double PrNonParity(double PrEr, double Qw)
        {
            return (1-Power(1-2*PrEr,Qw))/2;
        }

        // генерация случайной последовательности 0 и 1

      public  static byte TMIGenRand()
        {
            
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            //Random rnd = new Random((int)DateTime.Now.Ticks);
            double res = rnd.NextDouble();
            if (res < .5D)
                return 0;
            else
                return 1;
        }

        // генерация случайной последовательности контроль по (не)четности
      public static byte TMIGenPar(int TMIDigitalNumber, string TMIHelpLine, ref string TMIParamLine)
      {
          string res = TMIParamLine;
          int WordLong = Convert.ToInt32(ReadParam("WordLong", TMIHelpLine));
          byte CurrentDigital = TMIGenRand();
          // если параметр по модулю длины слова не равен 0 то
          if ((TMIDigitalNumber % WordLong) != 0)
          {
              
              int SumDg = Convert.ToInt32(ReadParam("SumDg", TMIParamLine)); //вначале=0
              SumDg += CurrentDigital; // складываем с генерируемым числом
              ChangeParam("SumDg", SumDg.ToString(), ref TMIParamLine);// изменяем число в строке
             
              return CurrentDigital;
          }
          else
          {
              int t = Convert.ToInt32(ReadParam("SumDg", TMIParamLine))%2;
              CurrentDigital = (byte)t;
             
              if (ReadParam("WordParity", TMIHelpLine) == "Н")
              {
                   CurrentDigital = (byte) (1 - CurrentDigital); 
              }
              ChangeParam("SumDg", "0", ref TMIParamLine);
              return CurrentDigital;
      
 
          }
      }
        // генерация случайной последовательности двойной контроль по (не)четности
      public static byte TMIGenDPar(int TMIDigitalNumber, string TMIHelpLine, ref string TMIParamLine)
      {
          byte res=0;
         
          int  WordLong = Convert.ToInt32(ReadParam("WordLong",TMIHelpLine));
          int BlockLong = Convert.ToInt32(ReadParam("BlockLong",TMIHelpLine));
          if (    (      (((TMIDigitalNumber-1) / WordLong) + 1)     % BlockLong ) != 0)
          {
              byte CurrentDigital = TMIGenPar(TMIDigitalNumber, TMIHelpLine, ref TMIParamLine);
              int SumBl = Convert.ToInt32(ReadParam("SumBl" + Convert.ToString(TMIDigitalNumber % WordLong), TMIParamLine)) + CurrentDigital;
              ChangeParam("SumBl" + (TMIDigitalNumber % WordLong).ToString(), SumBl.ToString(), ref TMIParamLine);
             
              res =  CurrentDigital;
          }
          else
          {
              if ((TMIDigitalNumber % WordLong) != 0)
              {
                   byte CurrentDigital =Convert.ToByte( Convert.ToInt32(ReadParam("SumBl" + Convert.ToString(TMIDigitalNumber % WordLong), TMIParamLine)) % 2);
                   
                  if (ReadParam("WordParity", TMIHelpLine) == "Н")
                  {
                      CurrentDigital = Convert.ToByte(1 - CurrentDigital);
                                     
                  }
                      ChangeParam("SumBl" + Convert.ToString(TMIDigitalNumber % WordLong), "0", ref TMIParamLine);
                      ChangeParam("SumDg" + Convert.ToString(Convert.ToInt32(ReadParam("SumDg", TMIParamLine)) + CurrentDigital), "0", ref TMIParamLine);
                     res = CurrentDigital;
              }
              else
               {
                  byte CurrentDigital = Convert.ToByte(Convert.ToInt32(ReadParam("SumDg", TMIParamLine)) % 2);
                   if (ReadParam("WordParity", TMIHelpLine) == "Н")
                   {
                       CurrentDigital = Convert.ToByte(1 - CurrentDigital);

                   }
                   ChangeParam("SumDg", "0", ref TMIParamLine);
                   res = CurrentDigital;
                  
               }
                  
      
             
 
          }
          return res;

      }

       // генерация символов ТМИ
        public static byte TMIGeneration(string TMISource, int TMIDigitalNumber, string TMIHelpLine , ref string TMIParamLine)
        {
            byte res = 255;

            if (TMISource == "Random")
                res = TMIGenRand();
            else if (TMISource == "Parity")
                res = TMIGenPar(TMIDigitalNumber, TMIHelpLine, ref TMIParamLine);
            else if (TMISource == "DoubleParity")
                res = TMIGenDPar(TMIDigitalNumber, TMIHelpLine, ref TMIParamLine);
            return res;

        }

        // Функция генерации ошибок
        public static byte ErrorGeneration(byte CurrentDigital, string DKParamLine )
        {
            float PrError = Convert.ToSingle(ReadParam("PrEr",DKParamLine));
            byte EstimateDigital = CurrentDigital;
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            double r = rand.NextDouble();
            if (r < PrError)
               EstimateDigital=Convert.ToByte(1 - CurrentDigital);
           
            return EstimateDigital;

        }


        // Модель Гильберта
        public static void GModel(byte CurrentDigital, string DKHelpLine, ref string DKParamLine, ref byte Symbol)
        {



            if (ReadParam("St", DKParamLine) == "0")
            {
                //Random rand = new Random(Guid.NewGuid().GetHashCode());
                Random rand = new Random((int)DateTime.Now.Ticks);
                if (rand.NextDouble() < Convert.ToDouble(ReadParam("PrCrBG", DKHelpLine)))
                {
                    ChangeParam("St", "1", ref DKParamLine);
                    ChangeParam("PrEr", ReadParam("PrErG", DKHelpLine), ref DKParamLine);
                }

            }
            else
            {
                Random rand1 = new Random(Guid.NewGuid().GetHashCode());
                if (rand1.NextDouble() < Convert.ToDouble(ReadParam("PrCrGB", DKHelpLine)))
                {
                    ChangeParam("St", "0", ref DKParamLine);
                    ChangeParam("PrEr", ReadParam("PrErB", DKHelpLine), ref DKParamLine);
                }
            }
            Symbol=ErrorGeneration(CurrentDigital, DKParamLine);

        }
    
      
        // Модель Бенета-Фройлиха
        public static void BFModel (byte CurrentDigital, string DKHelpLine , ref string DKParamLine, ref byte Symbol)
        {
            int PackCnt = Convert.ToInt32(ReadParam("St",DKParamLine));
            Random rand = new Random(Guid.NewGuid().GetHashCode());
             if (rand.NextDouble() < Convert.ToDouble(ReadParam("PrCrGB", DKHelpLine))) PackCnt++;
             if (rand.NextDouble() < Convert.ToDouble(ReadParam("PrCrBG", DKHelpLine))) PackCnt--;
            if (PackCnt < 0) PackCnt = 0;

            Symbol = CurrentDigital;
            ChangeParam("PrEr", Convert.ToString(PrNonParity(Convert.ToDouble(ReadParam("PrErB",DKHelpLine)), PackCnt)), ref DKParamLine);
            Symbol=ErrorGeneration(Symbol, DKParamLine);
            ChangeParam("St", PackCnt.ToString(), ref DKParamLine);


        }


        // Функция выбора модели канала передачи информации
        public static byte ChannelTransmit(string ChanModel, byte CurrentDigital, string DKHelpLine, ref string DKParamLine)
        {
            byte Symbol = 0;
            byte res = 0;
            switch (ChanModel)
            {
                case "WithOutMemory":
                    res = ErrorGeneration(CurrentDigital, DKParamLine);
                    break;
                case "Gilbert":
                    GModel(CurrentDigital,DKHelpLine, ref DKParamLine, ref Symbol);
                    res = Symbol;
                    break;
                case "EliotGilbert":
                    GModel(CurrentDigital,DKHelpLine,ref DKParamLine, ref Symbol);
                    res = Symbol;
                    break;
                case "BenetFroilih":
                    BFModel(CurrentDigital,DKHelpLine, ref DKParamLine,ref Symbol);
                    res = Symbol;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ChanModel");

            }

            return res;
        }

        // процедура оценки канала передачи критерий НЕЙМАНА_ПИРСОНА
        public static void ParityEstimateNaiman(byte EstimateDigital, int TMIDigitalNumber, string TMIHelpLine, ref string EstLine)
        {
            ChangeParam("StateEst","_", ref EstLine);
            int WordLong = Convert.ToInt32 (ReadParam("WordLong",TMIHelpLine));
            int EstDgSum = EstimateDigital+ Convert.ToInt32 (ReadParam("EstDgSum",EstLine));
            ChangeParam("EstDgSum",Convert.ToString(EstDgSum), ref EstLine);
            if ((TMIDigitalNumber % WordLong) == 0)
            {

                if (((ReadParam("WordParity", TMIHelpLine) == "Н") && (EstDgSum % 2 == 0)) ||
                    ((ReadParam("WordParity", TMIHelpLine) == "Ч") && (EstDgSum % 2 == 1)))
                {
                    ChangeParam("EstErSum", Convert.ToString(Convert.ToInt32(ReadParam("EstErSum", EstLine)) + 1), ref EstLine);
                }
                #region switch
                switch (Convert.ToInt32(ReadParam("StateQw",EstLine)))
                {
                    case 2:
                        if ( (TMIDigitalNumber - Convert.ToInt32(ReadParam("EstStart",EstLine))+1 )  >=  Convert.ToInt32(   ReadParam("SelLong12",EstLine))    )                                                        
                        {
                            if (Convert.ToInt32(ReadParam("EstErSum", EstLine)) >= Convert.ToInt32(ReadParam("Shield12", EstLine)))
                            {
                                ChangeParam("StateEst", "Плох", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);
                            }
                            else
                            {
                                ChangeParam("StateEst","Хор", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum","0", ref EstLine);
                            }
                        }
                      
                        break;
                    case 3:
                        if ( (TMIDigitalNumber - Convert.ToInt32(ReadParam("EstStart",EstLine))+1 )  >=  Convert.ToInt32(ReadParam("SelLong12",EstLine) ))
                        {
                            if (Convert.ToInt32(ReadParam("EstErSum", EstLine)) >= Convert.ToInt32(ReadParam("Shield12", EstLine)))
                            {
                                  ChangeParam("StateEst","Плох",ref EstLine);
                                  ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                  ChangeParam("EstErSum","0",ref EstLine);
                            }
                            else if ((Convert.ToInt32(ReadParam("EstErSum", EstLine)) <
                                           Convert.ToInt32(ReadParam("Shield12", EstLine)) &&
                                          (Convert.ToInt32(ReadParam("EstErSum", EstLine)) >=
                                             Convert.ToInt32(ReadParam("Shield23", EstLine))
                                           )

                                    ))
                            {
                                ChangeParam("StateEst", "Удов", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);

                            }
                            else
                            {
                                 ChangeParam("StateEst","Хор",ref EstLine);
                                 ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                 ChangeParam("EstErSum","0", ref EstLine);
                            }



                            
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("StateQw");


                }
                #endregion

                ChangeParam("EstDgSum", "0", ref EstLine);
            }
          
 
        }


        // процедура оценки канала передачи критерий ВАЛЬДА
        public static void ParityEstimateVald(byte EstimateDigital, int TMIDigitalNumber, string TMIHelpLine, ref string EstLine)
        {
             ChangeParam("StateEst","_", ref EstLine);
            int WordLong = Convert.ToInt32 (ReadParam("WordLong",TMIHelpLine));
            int EstDgSum = Convert.ToInt32(EstimateDigital) + Convert.ToInt32(ReadParam("EstDgSum", EstLine));
            ChangeParam("EstDgSum",EstDgSum.ToString(), ref EstLine);

            if ((TMIDigitalNumber % WordLong) == 0)
            {

                if (((ReadParam("WordParity", TMIHelpLine) == "Н") && (EstDgSum % 2 == 0)) ||
                    ((ReadParam("WordParity", TMIHelpLine) == "Ч") && (EstDgSum % 2 == 1)))
                {
                    ChangeParam("EstErSum", (Convert.ToInt32(ReadParam("EstErSum", EstLine)) + 1).ToString(), ref EstLine);
                }
                #region switch
                switch (Convert.ToInt32(ReadParam("StateQw", EstLine)))
                {
                    case 2:

                      


                        if (Convert.ToInt32(ReadParam("EstErSum", EstLine)) == Convert.ToInt32(ReadParam("Shield12", EstLine)))
                            {


                                ChangeParam("StateEst", "Плох", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);


                            }

                           


                            if ( (   TMIDigitalNumber - Convert.ToInt32(ReadParam("EstStart",EstLine))+1         ) >=
                                 (Convert.ToInt32( ReadParam("SelLong12",EstLine)  ) )
                           
                                )

                            {
                                ChangeParam("StateEst", "Хор", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);
                            }
                      

                        break;
                    case 3:
                       
                            if (ReadParam("EstErSum", EstLine) == ReadParam("Shield12", EstLine))
                            {
                                ChangeParam("StateEst", "Плох", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);
                            }

                            if (((TMIDigitalNumber - Convert.ToInt32(ReadParam("EstStart", EstLine)) + 1) >=
                                    (Convert.ToInt32(ReadParam("SelLong12", EstLine))))
                                   &&
                                   (ReadParam("EstErSum", EstLine) == ReadParam("Shield23", EstLine))
                                   )
                            {
                                ChangeParam("StateEst", "Удов", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);

                            }

                            if ((TMIDigitalNumber - Convert.ToInt32(ReadParam("EstStart", EstLine)) + 1) >=
                                  (Convert.ToInt32(ReadParam("SelLong23", EstLine)))

                                 )
                                  
                            {
                                ChangeParam("StateEst", "Хор", ref EstLine);
                                ChangeParam("EstStart", Convert.ToString(TMIDigitalNumber + 1), ref EstLine);
                                ChangeParam("EstErSum", "0", ref EstLine);
                            }




                       
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("StateQw");
                }
                #endregion

                ChangeParam("EstDgSum", "0", ref EstLine);
            }

           
 
        }

        // процедура выбора критерия оценки
        public static void ParityEstimate(byte EstimateDigital, int TMIDigitalNumber, string TMIHelpLine, ref string EstLine)
        {
             if (ReadParam("CrType",EstLine)=="Naiman")
             {
                ParityEstimateNaiman(EstimateDigital,
                                     TMIDigitalNumber,
                                     TMIHelpLine,
                                   ref EstLine);
             }

             if (ReadParam("CrType", EstLine) == "Vald")
             {
                 ParityEstimateVald(EstimateDigital,
                                    TMIDigitalNumber,
                                    TMIHelpLine,
                                    ref EstLine);
             }


        }

        //  процедура выбора генерации связанная с процедурой выбора критерия оценки
        public static void ChannelEstimate( byte EstimateDigital, string TMISource , int TMIDigitalNumber, string TMIHelpLine, ref string EstLine)
        {
            if (TMISource == "Random")
            {
                ChangeParam("StateEst", "No", ref EstLine);
            }
            else if ((TMISource == "Parity") || (TMISource == "DoubleParity"))
            {
                ParityEstimate(EstimateDigital,
                              TMIDigitalNumber,
                               TMIHelpLine,
                              ref EstLine);
            }

        }
                         



    }
}

