using Swan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Peripherals;
using Unosquare.WiringPi;

namespace RFPaymentSystem
{
   class Program
   {
      private const string ExitMessage = "Press Esc key to continue . . .";
      public static RFIDControllerMfrc522 device;
      private static byte[] cardUid;
      static async Task Main(string[] args)
	  {
		 Pi.Init<BootstrapWiringPi>();
         device = new RFIDControllerMfrc522(Pi.Spi.Channel0, 500000, Pi.Gpio[18]);
         cardUid = new byte[4];
         while(true)
		 {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("---------- Wilkommen ------------");
            await TestRfidController();
		 }
         
      }

      /// <summary>
      /// Tests the rfid controller.
      /// </summary>
      
      public static async Task TestRfidController()
      {
         Console.WriteLine("Karte einlesen");
         cardUid= await CardDetected();

         // Print UID
         Console.ForegroundColor = ConsoleColor.Green;
         Console.WriteLine($"Card UID: {cardUid[0]},{cardUid[1]},{cardUid[2]},{cardUid[3]}");
         Console.ForegroundColor = ConsoleColor.Blue;
         Console.WriteLine("Zum Bier kaufen erneut einlesen");

		 Task<Byte[]> WaitForCard=CardDetected();

         if (await Task.WhenAny(WaitForCard, Task.Delay(5000)) == WaitForCard)
         {
            // task completed within timeout

            //If we have same UID as last time, continue
            if (WaitForCard.Result[0] == cardUid[0]
               && WaitForCard.Result[1] == cardUid[1]
               && WaitForCard.Result[2] == cardUid[2]
               && WaitForCard.Result[3] == cardUid[3])
		    {
               Console.ForegroundColor = ConsoleColor.Blue;
               Console.WriteLine("Bier gekauft");
            }
            else
            {
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("Bitte die gleiche Karte erneut vorhalten.");
            }
         }
         else
         {
            // handle timeout
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("leider kein Bier");
		 }
 
         /*
            // Select the scanned tag
            device.SelectCardUniqueId(cardUid);
         */

         /*
            // Writing data to sector 1 blocks
            // Authenticate sector
            if (device.AuthenticateCard1A(RFIDControllerMfrc522.DefaultAuthKey, cardUid, 7) == RFIDControllerMfrc522.Status.AllOk)
            {
               var data = new byte[16 * 3];
               for (var x = 0; x < data.Length; x++)
               {
                  data[x] = (byte)(x + 10);
               }
               for (var b = 0; b < 3; b++)
               {
                  device.CardWriteData((byte)(4 + b), data.Skip(b * 16).Take(16).ToArray());
               }
            }
            */

         /*
            // Reading data
            var continueReading = true;
            for (var s = 0; s < 16 && continueReading; s++)
            {
               // Authenticate sector
               if (device.AuthenticateCard1A(RFIDControllerMfrc522.DefaultAuthKey, cardUid, (byte)((4 * s) + 3)) == RFIDControllerMfrc522.Status.AllOk)
               {
                  Console.WriteLine($"Sector {s}");
                  for (var b = 0; b < 3 && continueReading; b++)
                  {
                     var data = device.CardReadData((byte)((4 * s) + b));
                     if (data.Status != RFIDControllerMfrc522.Status.AllOk)
                     {
                        continueReading = false;
                        break;
                     }
                     Console.WriteLine($"  Block {b} ({data.Data.Length} bytes): {string.Join(" ", data.Data.Select(x => x.ToString("X2")))}");
                  }
               }
               else
               {
                  Console.WriteLine("Authentication error");
                  break;
               }
            }
            device.ClearCardSelection();
         */
      }

      private static async Task<byte[]> CardDetected()
      {
         while (true)
         {
            await Task.Delay(1000);
            // If a card is found
            if (device.DetectCard() != RFIDControllerMfrc522.Status.AllOk) continue;
            //Console.WriteLine("Card detected");

            // Get the UID of the card
            var uidResponse = device.ReadCardUniqueId();

            // If we have the UID, continue
            if (uidResponse.Status != RFIDControllerMfrc522.Status.AllOk) continue;
               
            return uidResponse.Data;
            // New Card detected
         }
      }

      private static void ReadAllCardSectors(bool readWithAuthentication)
      {
         Console.Clear();

         Console.WriteLine($"Testing RFID (Authentication={readWithAuthentication})");
         var device = new RFIDControllerMfrc522(Pi.Spi.Channel0, 500000, Pi.Gpio[18]);

         while (true)
         {
            // If a card is found
            if (device.DetectCard() != RFIDControllerMfrc522.Status.AllOk) continue;

            // Get the UID of the card
            var uidResponse = device.ReadCardUniqueId();

            // If we have the UID, continue
            if (uidResponse.Status != RFIDControllerMfrc522.Status.AllOk) continue;

            var cardUid = uidResponse.Data;

            // Print UID
            Console.WriteLine($"Card UID: {cardUid[0]},{cardUid[1]},{cardUid[2]},{cardUid[3]}");

            // Select the scanned tag
            device.SelectCardUniqueId(cardUid);

            // Reading data
            var continueReading = true;
            for (var s = 0; s < 16 && continueReading; s++)
            {
               // Authenticate sector - not required for MIFARE Ultralight
               if (!readWithAuthentication || device.AuthenticateCard1A(RFIDControllerMfrc522.DefaultAuthKey, cardUid, (byte)((4 * s) + 3)) == RFIDControllerMfrc522.Status.AllOk)
               {
                  Console.WriteLine($"Sector {s}");
                  for (var b = 0; b < 3 && continueReading; b++)
                  {
                     var data = device.CardReadData((byte)((4 * s) + b));
                     if (data.Status != RFIDControllerMfrc522.Status.AllOk)
                     {
                        continueReading = false;
                        break;
                     }

                     Console.WriteLine($"  Block {b} ({data.Data.Length} bytes): {string.Join(" ", data.Data.Select(x => x.ToString("X2")))}");
                  }
               }
               else
               {
                  Console.WriteLine($"Authentication error, sector {s}");
               }
            }

            device.ClearCardSelection();

            while (true)
            {
               var input = Console.ReadKey(true).Key;
               if (input != ConsoleKey.Escape) continue;

               break;
            }
            break;
         }
         }
      }
   }
