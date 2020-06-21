using System;
using System.Text;
using System.Net;
using System.Xml;

namespace TBike2OSM
{
    class MainProcess
    {
        /// <summary>
        /// 資料介接網址
        /// </summary>
        public const string DATA_URL = "http://tbike-data.tainan.gov.tw/Service/StationStatus/Xml";

        /// <summary>
        /// 命令
        /// </summary>
        private static string _command = "";

        /// <summary>
        /// 輸入的檔案名稱
        /// </summary>
        private static string _compareFileName = "";

        private static string _NewCompareFile = "";
        private static string _OldCompareFile = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Tainan T-Bike Data Tool v1.0 by Kagami");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("1.DownLoad new data and save.");
            Console.WriteLine("2.Compare new data with other data , and generate compare data.");
            Console.WriteLine("3.Compare two datas in local");
            Console.WriteLine("Please Choose with number:");
            _command = Console.ReadLine();


            switch (_command)
            {
                //1.轉換系統
                case "1":
                    {
                        //下載檔案
                        TBikeDataDownloader _downloader = new TBikeDataDownloader(DATA_URL);
                        //下載成功
                        if (_downloader.strUnFormatedData != "")
                        {
                            //轉換格式
                            TBikeDataConverter _converter = new TBikeDataConverter(_downloader.strUnFormatedData);

                            string result = _converter.SaveData();

                            if (result != "")
                                Console.WriteLine(result + "Saved.");
                        }
                    }
                    break;

                //2.差異系統
                case "2":
                    {
                        Console.WriteLine("Press file name without extension:");
                        _compareFileName = Console.ReadLine();

                        //讀檔
                        XmlDocument _old = new XmlDocument();
                        try
                        {
                            _old.Load(_compareFileName + ".xml");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to load file! Please check input name is vaild or the file is exist.");
                            break;
                        }

                        //XML為有效的才做
                        if (_old.HasChildNodes)
                        {
                            Console.WriteLine("Load file Successuful!");

                            TBikeDataDownloader _downloader = new TBikeDataDownloader(DATA_URL);
                            if (_downloader.strUnFormatedData != "")
                            {
                                TBikeDataConverter _converter = new TBikeDataConverter(_downloader.strUnFormatedData);

                                TBikeDataComparer _comparer = new TBikeDataComparer(_converter.FormatedData, _old);

                                string result = _comparer.SaveData();

                                if (result != "")
                                    Console.WriteLine(result + "Saved.");
                            }
                        }
                    }
                    break;
                //3.差異系統(本地比較)
                case "3":
                    {
                        Console.WriteLine("Press NEWER file name without sub-file name:");
                        _NewCompareFile = Console.ReadLine();

                        XmlDocument _new = new XmlDocument();
                        try
                        {
                            _new.Load(_NewCompareFile + ".xml");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to load file! Please check input name is vaild or the file is exist.");
                            break;
                        }

                        Console.WriteLine("Press OLDER file name without sub-file name:");
                        _OldCompareFile = Console.ReadLine();

                        XmlDocument _old = new XmlDocument();
                        try
                        {
                            _old.Load(_OldCompareFile + ".xml");
                        }
                        catch
                        {
                            Console.WriteLine("Failed to load file! Please check input name is vaild or the file is exist.");
                            break;
                        }

                        if (_old.HasChildNodes && _new.HasChildNodes)
                        {
                            Console.WriteLine("Load file Successuful!");

                            TBikeDataComparer _comparer = new TBikeDataComparer(_new, _old);

                            string result = _comparer.SaveData();

                            if (result != "")
                                Console.WriteLine(result + "Saved.");
                        }
                    }
                    break;
                //Etc. 輸入無效的指令
                default:
                    {
                        Console.WriteLine("Not Availeble Command.");
                    }
                    break;
            }

            //清除資料
            _command = "";
            _compareFileName = "";

            _NewCompareFile = "";
            _OldCompareFile = "";

            //結束程式
            Console.WriteLine("Press Any Key to Exit!");
            Console.ReadLine();
        }
    }

    public class TBikeDataDownloader
    {
        /// <summary>
        /// 尚未整理的 XML 格式字串
        /// </summary>
        public string strUnFormatedData
        {
            get
            {
                if (_unformatedData != "")
                    return _unformatedData;
                else
                    return "";
            }
        }
        private string _unformatedData;

        /// <summary>
        /// 建構子，初始化下載器
        /// </summary>
        /// <param name="url">下載網址</param>
        public TBikeDataDownloader(string url)
        {
            try
            {
                Console.WriteLine("Downloading......");
                WebClient wc = new WebClient();
                wc.Credentials = CredentialCache.DefaultCredentials;

                Byte[] unClearedXMLByte = wc.DownloadData(url);

                _unformatedData = Encoding.UTF8.GetString(unClearedXMLByte);

                wc.Dispose();
            }
            catch (WebException e)
            {
                Console.WriteLine("Download Fail ! Code:" + e.Message);
            }
        }
    }

    public class TBikeDataComparer
    {
        /// <summary>
        /// 比較完後的XML
        /// </summary>
        public XmlDocument ComparedDoc
        {
            get
            {
                return _compareDoc;
            }
        }
        private XmlDocument _compareDoc = new XmlDocument();

        /// <summary>
        /// 建構子，初始化XML比較器
        /// </summary>
        /// <param name="_new">新資料</param>
        /// <param name="_old">舊資料</param>
        public TBikeDataComparer(XmlDocument _new, XmlDocument _old)
        {
            Compare(_new, _old);
            //CollectData(_new, _old);
        }

        /// <summary>
        /// 儲存XML檔案
        /// </summary>
        /// <returns></returns>
        public string SaveData()
        {
            string _fileName = DateTime.Now.ToString("yyyyMMdd") + "_ComparedTbike.xml";

            try
            {
                _compareDoc.Save(_fileName);
                return _fileName;
            }
            catch (XmlException e)
            {
                Console.WriteLine("Compared data fail to save , check your collected xml format is vaild.");
                Console.WriteLine("Code:" + e.Message);
                return "";
            }
        }

        /// <summary>
        /// 比較兩OSM XML 
        /// </summary>
        /// <param name="_new">新資料</param>
        /// <param name="_old">舊資料</param>
        private bool Compare(XmlDocument _new, XmlDocument _old)
        {
            Console.WriteLine("Start compare !");

            XmlDocument _newDoc = _new;
            XmlNodeList _newDataNodes = _newDoc.SelectNodes("osm/node");
            XmlNode _newParentNode = _newDataNodes.Item(0).ParentNode;

            XmlDocument _oldDoc = _old;
            XmlNodeList _oldDataNodes = _oldDoc.SelectNodes("osm/node");
            XmlNode _oldParentNode = _oldDataNodes.Item(0).ParentNode;

            //旗標，確認是否需要刪除新資料的XML節點
            bool bIsNeedToDelNewData = false;

            for (int i = 0; i < _newDataNodes.Count; ++i)
            {
                for (int j = 0; j < _oldDataNodes.Count; ++j)
                {
                    //Console.WriteLine(_oldDataNodes.Item(j).Attributes["id"].Value);
                    //Console.WriteLine("---------------");
                    //Console.WriteLine(_newDataNodes.Item(i).Attributes["id"].Value);

                    XmlNodeList _NewDataChilds = _newDataNodes.Item(i).ChildNodes;
                    XmlNodeList _OldDataChilds = _oldDataNodes.Item(j).ChildNodes;

                    string _newDataRef = _NewDataChilds.Item(2).Attributes["v"].Value;
                    string _oldDataRef = _OldDataChilds.Item(2).Attributes["v"].Value;

                    //新資料節點和舊資料節點一樣
                    if (_oldDataRef == _newDataRef)
                    {
                        //刪除舊資料節點
                        XmlNode _shouldRemoveNode = _oldDataNodes.Item(j);
                        _oldParentNode.RemoveChild(_shouldRemoveNode);

                        //標記新資料也同樣要刪除
                        bIsNeedToDelNewData = true;
                    }
                    else
                    {
                        //不一樣就繼續做
                        continue;
                    }
                }

                //旗標開啟
                if (bIsNeedToDelNewData)
                {
                    //刪除新資料內的相同節點
                    XmlNode _shouldRemoveNode = _newDataNodes.Item(i);
                    _newParentNode.RemoveChild(_shouldRemoveNode);

                    //關閉旗標
                    bIsNeedToDelNewData = false;
                }
            }

            //結束後檢查是否雙方都被刪到沒資料
            if (_newDataNodes.Count == 0 && _oldDataNodes.Count == 0)
            {
                Console.WriteLine("Two files is the same.");
                return false;
            }
            else
            {
                Console.WriteLine("Compared Completed !");

                //整理資料
                CollectData(_newDoc, _oldDoc);
                return true;

                //Console.WriteLine(_newDataNodes.Count);
                //Console.WriteLine(_oldDataNodes.Count);
            }
        }

        /// <summary>
        /// 整理比對後的資料
        /// </summary>
        /// <param name="_ComparedNew">新資料</param>
        /// <param name="_ComparedOld">舊資料</param>
        private void CollectData(XmlDocument _ComparedNew, XmlDocument _ComparedOld)
        {
            Console.WriteLine("Collect Data Start!");

            XmlNodeList _newDataNodes = _ComparedNew.SelectNodes("osm/node");
            XmlNodeList _oldDataNodes = _ComparedOld.SelectNodes("osm/node");

            XmlDocument _collectedData = new XmlDocument();

            //產生XML宣告
            XmlDeclaration dec = _collectedData.CreateXmlDeclaration("1.0", "UTF-8", null);

            _collectedData.AppendChild(dec);

            //產生OSM標頭
            XmlElement osm = _collectedData.CreateElement("osm");
            osm.SetAttribute("version", "0.6");
            osm.SetAttribute("generator", "kagami");

            for (int i = 0; i < _oldDataNodes.Count; ++i)
            {
                //舊資料還有留存的XML節點，即為移除的站點
                //加上FIXME TAG 讓Mapper知道要移除此處
                XmlElement _fixme = _ComparedOld.CreateElement("tag");
                _fixme.SetAttribute("k", "fixme");
                _fixme.SetAttribute("v", "This station has been removed.");

                _oldDataNodes.Item(i).AppendChild(_fixme);

                //資料塞入XML
                XmlNode _modifiedNode = _collectedData.ImportNode(_oldDataNodes.Item(i), true);
                osm.AppendChild(_modifiedNode);
            }

            for (int j = 0; j < _newDataNodes.Count; ++j)
            {
                //新資料留存的XML節點，即為新增的站點
                //不必做任何處理，資料直接塞入XML
                XmlNode _modifiedNode = _collectedData.ImportNode(_newDataNodes.Item(j), true);
                osm.AppendChild(_modifiedNode);
            }

            _collectedData.AppendChild(osm);

            //輸出XML
            _compareDoc = _collectedData;
        }
    }

    public class TBikeDataConverter
    {
        /// <summary>
        /// 轉換後的OSM XML
        /// </summary>
        public XmlDocument FormatedData
        {
            get
            {
                return _formatedData;
            }
        }
        private XmlDocument _formatedData = new XmlDocument();

        /// <summary>
        /// 建構子，初始化XML轉換器
        /// </summary>
        /// <param name="OriginalData">原始XML字串</param>
        public TBikeDataConverter(string OriginalData)
        {
            _formatedData = ReadString2OSMXML(OriginalData);
        }

        /// <summary>
        /// 儲存OSM XML檔案
        /// </summary>
        /// <returns></returns>
        public string SaveData()
        {
            string _fileName = DateTime.Now.ToString("yyyyMMdd") + "_Tbike.xml";

            try
            {
                _formatedData.Save(_fileName);
                return _fileName;
            }
            catch (XmlException e)
            {
                Console.WriteLine("Data Save Failed ! Check your xml format is vailed.");
                Console.WriteLine("Code:" + e.Message);
                return "";
            }
        }

        /// <summary>
        /// 轉換為 OSM　XML格式
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        private XmlDocument ReadString2OSMXML(string origin)
        {
            Console.WriteLine("Convert Start !");

            //讀取原始XML資料
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(origin);

            //抓到所有車站的Node
            XmlNodeList stations = doc.SelectNodes("ArrayOfStationStatusData/StationStatusData");

            //新增OSM 格式的XML
            XmlDocument resultdoc = new XmlDocument();

            //產生XML宣告
            XmlDeclaration dec = resultdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

            resultdoc.AppendChild(dec);

            //產生OSM標頭
            XmlElement osm = resultdoc.CreateElement("osm");
            osm.SetAttribute("version", "0.6");
            osm.SetAttribute("generator", "kagami");

            for (int i = 0; i < stations.Count; ++i)
            {
                //產生Node和屬性
                XmlElement node = resultdoc.CreateElement("node");

                node.SetAttribute("id", "-" + (i + 500).ToString());
                node.SetAttribute("lat", doc.ImportNode(stations.Item(i)["Latitude"], true).InnerText);
                node.SetAttribute("lon", doc.ImportNode(stations.Item(i)["Longitude"], true).InnerText);
                node.SetAttribute("visible", "true");
                node.SetAttribute("version", "1");

                //產生底下的Tag
                XmlElement staName = resultdoc.CreateElement("tag");
                staName.SetAttribute("k", "name");
                staName.SetAttribute("v", doc.ImportNode(stations.Item(i)["StationName"], true).InnerText);

                XmlElement staRef = resultdoc.CreateElement("tag");
                staRef.SetAttribute("k", "ref");
                staRef.SetAttribute("v", doc.ImportNode(stations.Item(i)["Id"], true).InnerText);

                XmlElement staNote = resultdoc.CreateElement("tag");
                staNote.SetAttribute("k", "description");
                staNote.SetAttribute("v", doc.ImportNode(stations.Item(i)["Address"], true).InnerText);

                XmlElement capacity = resultdoc.CreateElement("tag");
                capacity.SetAttribute("k", "capacity");
                capacity.SetAttribute("v", doc.ImportNode(stations.Item(i)["Capacity"], true).InnerText);

                XmlElement staOperator = resultdoc.CreateElement("tag");
                staOperator.SetAttribute("k", "operator");
                staOperator.SetAttribute("v", "臺南市政府交通局");

                XmlElement staNetwork = resultdoc.CreateElement("tag");
                staNetwork.SetAttribute("k", "network");
                staNetwork.SetAttribute("v", "TBike");

                XmlElement amenity = resultdoc.CreateElement("tag");
                amenity.SetAttribute("k", "amenity");
                amenity.SetAttribute("v", "bicycle_rental");

                XmlElement creditCard = resultdoc.CreateElement("tag");
                creditCard.SetAttribute("k", "payment:credit_cards");
                creditCard.SetAttribute("v", "yes");

                XmlElement website = resultdoc.CreateElement("tag");
                website.SetAttribute("k", "website");
                website.SetAttribute("v", "http://tbike.tainan.gov.tw/Portal");

                node.AppendChild(amenity);
                node.AppendChild(staName);
                node.AppendChild(staRef);
                node.AppendChild(capacity);
                node.AppendChild(staOperator);
                node.AppendChild(staNetwork);
                node.AppendChild(staNote);
                node.AppendChild(website);
                node.AppendChild(creditCard);

                osm.AppendChild(node);
            }

            resultdoc.AppendChild(osm);

            return resultdoc;
        }
    }
}
