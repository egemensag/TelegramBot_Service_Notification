using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace WebApplication3.Controllers {
    public class HomeController : Controller {
        static TelegramDbEntities db = new TelegramDbEntities();
        static ITelegramBotClient botClient = new TelegramBotClient("---BOT TOKEN BİLGİSİ---");//Oluşturulan botun token bilgisi.
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public JsonResult Get(string _phoneNumber,string text)//Get fonksiyonu içine iki parametre geliyor.. gelen phoneNumber Db de aranıyor ve telefon numarasına karşılık gelen kayıttaki chat id si alınıyor.Sonrasında gelen text stringi bu chat id vasıtasıyla gönderiliyor.
        {
            var x = db.ClientInfos.Where(p=>p.PhoneNumber == _phoneNumber).FirstOrDefault();//Db den phone number'a ait olan satırı buluyor
            var result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = x;
            botClient.SendTextMessageAsync(x.ChatID, text);//mesajı gönderiyor.
            return result;
        }

        [HttpPost]
        public static void Post()//Post methodu sadece dinleme yapıyor.Licrus uygulaması üzerinden telegrama kayıt ol şeklinde bir button'a basıldığında bu method çalışıp ilk kaydı yapıcak.
        {
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
        }//İlk kayıt harici post operasyonu hiç kullanılmayacak.Onun dışında servis mesajı bildirimleri için hep get methodu kullanılıcak.

        static void Bot_OnMessage(object sender,MessageEventArgs e)//Botun dinleme yaptığı fonksiyon
        {
            string _message = e.Message.Text.ToString();//Bota gelen mesajı string e çeviriyoruz.ilk kayıt mesajımız "/start 905....." şeklinde olduğu için ilk 7 karakteri siliyoruz.
            if (!(_message.Substring(0,7).Equals("/start ")))//ilk kayıt mesajı değilse yazılanlara cevap verilmesin.
            {
                botClient.SendTextMessageAsync(e.Message.Chat.Id,
                    "Hello " + e.Message.Chat.FirstName + " " + e.Message.Chat.LastName);//ilk kayıt mesajı değilse sadece "Hello _FirstName_ _LastName_" şeklinde bir mesaj gönderiyor.Istenilirse hiç birşey yapılmayadabilir.Sadece bot aktif olarak konuşuyor gibi dursun diye eklemiştim.
            }
            else//ilk kayıt ise yani uygulamadan ilk defa aktif etme tuşuna basıldıysa  bu else bloğu çalışacak
            {
                ClientInfos _clientInfos = new ClientInfos();
                if (db.ClientInfos.Find(e.Message.Chat.Id) == null)//gelen mesajdaki chat id bulunup DB de sorgulanıyor.ilk kayıt olduğu teyit ediliyor
                {
                    _message = _message.Substring(7);//mesajdaki "/start " kısmı silinip Db ye kayıt işlemi yapılıyor.
                    _clientInfos.ChatID = e.Message.Chat.Id;
                    _clientInfos.ClientFirstName = e.Message.Chat.FirstName;
                    _clientInfos.ClientLastName = e.Message.Chat.LastName;
                    _clientInfos.ClientUserName = e.Message.Chat.Username;
                    _clientInfos.PhoneNumber = _message;
                    db.ClientInfos.Add(_clientInfos);
                    db.SaveChanges();
                }
                else
                {
                    botClient.SendTextMessageAsync(e.Message.Chat.Id, "Hello " + e.Message.Chat.FirstName + " " + e.Message.Chat.LastName);
                }
            }
        }
    }
}
