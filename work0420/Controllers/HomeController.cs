using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using work0420.Models;
using HtmlAgilityPack;

namespace work0420.Controllers
{
    public class HomeController : Controller
    {
        private DB0420_newsEntities db = new DB0420_newsEntities();
        // GET: Home 直接先存入SQL
        public ActionResult Index()
        {
            HtmlWeb objUDN = new HtmlWeb();
            HtmlWeb objCNA = new HtmlWeb();

            HtmlDocument docU = objUDN.Load("https://udn.com/news/index");
            HtmlDocument docC = objCNA.Load("https://www.cna.com.tw/");
            //return Content("OK"); //測試load是否成功

            HtmlNode rootU = docU.DocumentNode;
            var getU = rootU.SelectNodes("//*[@id=\"tab1\"]/dl/dt");

            //var s = "";
            //foreach (var data in getU) { s = s + data.InnerText + "<br>"; }

            HtmlNode rootC = docC.DocumentNode;
            var getC = rootC.SelectNodes("//*[@id=\"myLatesNews\"]/li");

            //s = s + "<p>";
            //foreach (var data in getC) { s = s + data.InnerText + "<br>"; }

            //return Content(s.ToString());  //測試找出的規律是否正確

            var today = DateTime.Now.ToString("yyyy/MM/dd");
            
            foreach (var data in getU)
            {
                var item = new Models.newsDB();
                item.date = today;
                item.time = data.SelectSingleNode("./a").InnerText.Substring(0, 5);
                item.title = data.SelectSingleNode("./a").InnerText.Substring(5);
                item.urlwith = data.SelectSingleNode("./a").Attributes["href"].Value;
                item.state = "1";
                item.wherefrom = "u";
                db.newsDBs.Add(item);
                var mark = 0; //記錄資料沒有重覆
                foreach (var o in db.newsDBs)
                {
                    if (o.title == item.title)
                    {
                        mark = 1;  //若此筆記錄在資料庫裡出現過,mark=1表示重覆了,跳出foreach
                        break;
                    }
                }
                if (mark == 1) {mark = 0; break; }
                db.SaveChanges();
            }

            foreach (var data in getC)
            {
                var item = new Models.newsDB();
                item.date = today;
                item.title = data.SelectSingleNode("./a").InnerText.Substring(7);
                item.urlwith = data.SelectSingleNode("./a").Attributes["href"].Value;
                item.time = data.SelectSingleNode("./a").InnerText.Substring(2, 5);
                item.state = "1";
                item.wherefrom = "c";
                db.newsDBs.Add(item);
                var mark = 0; //記錄資料沒有重覆
                foreach (var o in db.newsDBs)
                {
                    if (o.title == item.title)
                    {
                        mark = 1;  //若此筆記錄在資料庫裡出現過,mark=1表示重覆了,跳出foreach
                        break;
                    }
                }
                if (mark == 1) { mark = 0; break; }
                db.SaveChanges();

            }
        
            //以上將網路新聞自動存入資料庫******************************************************************

            ////製做view需要的資料,分成newsListU聯合新聞網 及以 newsListC中央社新聞

            var newsListU = from o in db.newsDBs
                            where o.date == today && o.wherefrom == "u"
                            orderby o.time descending
                            select o;
            var newsListC = from o in db.newsDBs
                            where o.date == today && o.wherefrom == "c"
                            orderby o.time descending
                            select o;

            ViewBag.u = newsListU.Take(8).ToList();
            ViewBag.c = newsListC.Take(8).ToList();


            //將資料state=2收藏的,傳給view
            var newsList2 = from o in db.newsDBs
                            where o.state == "2"
                            orderby o.date descending, o.time descending
                            select o;
            return View(newsList2.ToList());
        }

       

        // GET: Home/Edit/5
        public ActionResult Edit(int? id)
        {
            newsDB item = db.newsDBs.Find(id);
            return View(item);
        }

        // POST: Home/Edit/5
        [HttpPost]
        public ActionResult Edit(newsDB n)
        {
            if (Request.Form["okOrCancel"] == "Cancel")
            {
                return RedirectToAction("Index");   //如果按了取消鍵-->什麼都不做跳回首頁
            }
            newsDB item = db.newsDBs.Find(n.no); //找到要收藏的資料no
            item.no = n.no;
            item.date = n.date;
            item.time = n.time;
            item.title = n.title;
            item.urlwith = n.urlwith;
            item.state = "2";
            item.wherefrom = n.wherefrom;  
            //全部欄位都放一次避免NULL值
            db.SaveChanges();
            return RedirectToAction("Index");
        }
         
   
    }
}
