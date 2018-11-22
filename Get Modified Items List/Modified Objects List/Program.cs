using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aras.IOM;
using System.Collections;
using System.IO;
using Microsoft.Web.Administration;
using System.Xml;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new Form1());
        }

        //HttpServerConnection conn = null;
        internal static HttpServerConnection login(string ArasURL, string DataBaseName, string userid, string usePass)
        {

            String[] urlarray = ArasURL.Split('/');

            String ServerName = urlarray[2];
            String Site = urlarray[3];

            String url = @"http://" + ServerName + "/" + Site;
            //String url = ArasURL;
            String db = DataBaseName;
            String user = userid;
            String password = usePass;
            HttpServerConnection conn = IomFactory.CreateHttpServerConnection(url, db, user, password);
            Item login_result = conn.Login();
            if (login_result.isError())
            {
                //return null;
                MessageBox.Show(login_result.getErrorString().Replace("SOAP-ENV:ServerAuthentication failed for admin",""),"Login Failed");
                return null;
                //throw new Exception("Login failed :-" + login_result.getErrorString());
            }

            return conn;
        }
        internal static Aras.IOM.HttpServerConnection login()
        {
            return null; ;
        }

        internal static List<string> getAllDatabases(string URL)
        {
            List<String> DBList = new List<String>();
            String val = URL;
            String[] urlarray = URL.Split('/');

            String ServerName = urlarray[2];
            String Site = urlarray[3];

            String innovatorxmlpath = @"http://" + ServerName + "/" + Site + "/Client/innovator.xml";

            XmlDocument innovatorxml = new XmlDocument();
            innovatorxml.Load(innovatorxmlpath);

            string path = innovatorxml.SelectSingleNode("/ConfigFilePath/@value").Value;
            XmlDocument ConfigFile = new XmlDocument();
            if (ServerName.ToLower().Contains("localhost"))
            {
                
            }
            else
            {
                path = @"\\"+ServerName+"\\" + path.Replace(":","$");
            }
            ConfigFile.Load(path);

            XmlNodeList DBNodelist = ConfigFile.SelectNodes("//DB-Connection");

            foreach (XmlNode DBnode in DBNodelist)
            {
                DBList.Add(DBnode.Attributes["id"].Value);
            }

            return DBList;
        }
        internal static List<String> getAllDatabases()
        {
            List<String> DBList = new List<String>();
            using(ServerManager mgr = ServerManager.OpenRemote("Some-Server")) 
            {
    
              Configuration config = mgr.GetWebConfiguration("site-name", "/test-application");
    
              ConfigurationSection appSettingsSection = config.GetSection("appSettings");
    
              ConfigurationElementCollection appSettingsCollection = appSettingsSection.GetCollection();
    
              ConfigurationElement addElement = appSettingsCollection.CreateElement("add");
              addElement["key"] = @"NewSetting1";
              addElement["value"] = @"SomeValue";
              appSettingsCollection.Add(addElement);
               
    
      //serverManager.CommitChanges();
            }
             return DBList;
        }

        internal static String ListAllMdifiedObjects(string date, string path, HttpServerConnection conn)
        {
            Innovator inn = IomFactory.CreateInnovator(conn);
            String in_ModDate = date;
            //in_ModDate ="8/23/2015";
            DateTime dt = Convert.ToDateTime(in_ModDate);

            String ModDate = dt.ToString("s");

            //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " ModDate --> " + ModDate);

            String HTML = "Query for date = " + in_ModDate + "<style>table, th, td {" +
                                        "    border: 1px solid black;" +
                                        "    border-collapse: collapse;" +
                                        "}</style><table align='center' cellpadding='5'>" +
                                        "<tr bgcolor='#D7D4F0' style='font-weight:bold;align=center;'>" +
                                        "<td></td><td>S.No</td><td>Type</td><td>Name</td><td>Created On</td><td>Modified On</td><td>Modified By</td><td>Package Definition</td></tr>";

            int cnt = 1;

            String aConfigPath = System.Windows.Forms.Application.ExecutablePath + ".config";
            //---------------get the List of Items
            XmlNode xmlNode;
            XmlNodeList ItemList;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(aConfigPath);
            xmlNode = xmlDoc.DocumentElement.SelectSingleNode("//ItemsList");
            ItemList = xmlNode.SelectNodes("Item");

            List<String> Datatypes = new List<String>();

            foreach (XmlNode ItemXML in ItemList)
            {
                String ItemName = ItemXML.InnerText;
                Datatypes.Add(ItemName);
            }
            //--------------------------------------


           

            //Datatypes.Add("Action");
            //Datatypes.Add("EMail Message");
            //Datatypes.Add("FileType");
            //Datatypes.Add("Form");
            //Datatypes.Add("Grid");
            //Datatypes.Add("History Template");
            //Datatypes.Add("Identity");
            //Datatypes.Add("ItemType");
            //Datatypes.Add("Life Cycle Map");
            //Datatypes.Add("List");
            //Datatypes.Add("Method");
            //Datatypes.Add("Permission");
            //Datatypes.Add("RelationshipType");
            //Datatypes.Add("RelationshipTypeItemType");
            //Datatypes.Add("Report");
            //Datatypes.Add("Sequence");
            //Datatypes.Add("SQL");
            //Datatypes.Add("Workflow Map");

            Hashtable Identityinfo = new Hashtable();

            String RelShipList = "";
            foreach (String ItemType in Datatypes)
            {


                //String ItemTyep = "Method";
                //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", "*************************************************************************************************");
                //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " ItemType --> " + ItemType);
                String RealItemType = ItemType;
                String ReportItemType = ItemType;
                if (ItemType == "RelationshipTypeItemType")
                {
                    RealItemType = "ItemType";
                    ReportItemType = "RelationshipType";
                }
                String aml = "<AML><Item type='" + RealItemType + "' action='get' select='*'>" +
                                    "<modified_on condition='ge'>" + ModDate + "</modified_on>";
                if (ItemType == "ItemType")
                {
                    aml += "<is_relationship>0</is_relationship>";
                }
                if (ItemType == "RelationshipTypeItemType")
                {
                    aml += "<is_relationship>1</is_relationship>";
                }
                aml += "</Item></AML>";



                Item Results = inn.applyAML(aml);

                ////CCO.Utilities.WriteDebug("List the Last Modified Elements.html"," Results --> "+Results.getItemCount() +"\n"+Results);
                for (int i = 0; i < Results.getItemCount(); i++)
                {
                    Item Result = Results.getItemByIndex(i);
                    String keyed_name = Result.getProperty("keyed_name");
                    keyed_name = keyed_name.Replace("&", "&amp;");
                    //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " keyed_name --> " + keyed_name);
                    String Mod_date = Result.getProperty("modified_on");
                    String created_date = Result.getProperty("created_on");
                    String Mod_by_id = Result.getProperty("modified_by_id");
                    //return inn.newError(Mod_by_id);
                    String Mod_by = "";

                    if (ItemType == "RelationshipType")
                    {
                        RelShipList += keyed_name+",";
                    }

                    if (ItemType == "RelationshipTypeItemType" && RelShipList.Contains(keyed_name))
                    {
                        continue;
                    }

                    if (!Identityinfo.Contains(Mod_by_id))
                    {

                        /*	String sqlQry = "select * from innovator.[User] where id ='"+Mod_by_id+"'";
                            Item Ident = inn.applySQL(sqlQry);*/
                        Item Ident = inn.getItemById("User", Mod_by_id);

                        if (Ident.getItemCount() > 0)
                        {
                            Mod_by = Ident.getItemByIndex(0).getProperty("keyed_name");
                            //return inn.newError(Mod_by);
                            Identityinfo.Add(Mod_by_id, Mod_by);
                        }

                    }
                    else
                    {
                        Mod_by = Identityinfo[Mod_by_id].ToString();
                    }



                    //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " Mod_date --> " + keyed_name);
                    String ItemconfigId = Result.getProperty("config_id");
                    String ItemId = Result.getProperty("id");

                    String Packaml = "<AML>" +
                                                "  <Item type='PackageDefinition' action='get' select='keyed_name'>" +
                                                "    <Relationships>" +
                                                "      <Item type='PackageGroup' action='get' select='keyed_name,name'>" +
                                                "        <Relationships>" +
                                                "          <Item type='PackageElement' action='get' select='*'>" +
                                                "           <or>" +
                                                "                <element_id>" + ItemId + "</element_id>" +
                                                "                <element_id>" + ItemconfigId + "</element_id>" +
                                                "               <name>" + keyed_name + "</name>" +
                                                "           </or>" +
                                                "           <element_type>"+ RealItemType + "</element_type>" +
                                                "          </Item>" +
                                                "        </Relationships>" +
                                                "      </Item>" +
                                                "    </Relationships>" +
                                                "  </Item>" +
                                                "</AML>";
                    //	//CCO.Utilities.WriteDebug("List the Last Modified Elements.html"," Packaml --> "+Packaml);	
                    Item PackDef = inn.applyAML(Packaml);
                    String bgclr = "";
                    String PackDefName = "";
                    if (PackDef.getItemCount() == 1)
                    {
                        bgclr = "#2BB34F";
                        PackDefName = PackDef.getItemByIndex(0).getProperty("keyed_name");
                    }
                    else if (PackDef.getItemCount() > 1)
                    {
                        bgclr = "#FF0000";
                        for (int packcnt = 0; packcnt < PackDef.getItemCount(); packcnt++)
                        {
                            if (packcnt != 0)
                                PackDefName += "<br>";
                            PackDefName += PackDef.getItemByIndex(packcnt).getProperty("keyed_name");
                        }
                    }
                    else
                    {
                        bgclr = "#FBF29C";
                        PackDefName = PackDef.getItemCount() + "";
                    }

                    //CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " PackDefName --> " + PackDefName);


                    HTML += "<tr><td><input type='checkbox'/></td><td>" + cnt + "<td>" + ReportItemType + "</td><td>" + keyed_name + "</td><td>" + created_date + "</td><td>" + Mod_date + "</td><td>" + Mod_by + "</td><td bgcolor='" + bgclr + "'>" + PackDefName + "</td></tr>";
                    cnt++;
                }
            }
            HTML += "</table>";

            //return inn.newError(DateTime.Now.ToString("s").Replace("-","_").Replace(":","_");
            String FileName = "Package_" + DateTime.Now.ToString("s").Replace("-", "_").Replace(":", "_") + ".htm";
            //string path = @"C:\Aras\Aras 10Sp4\Innovator\Server\Modification List" + FileName;

            path = path + "\\" + FileName;

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                using (TextWriter tw = new StreamWriter(path))
                {
                    tw.WriteLine(HTML);
                    tw.Close();
                }

            }

            else if (File.Exists(path))
            {
                using (TextWriter tw = new StreamWriter(path))
                {
                    tw.WriteLine(HTML);
                    tw.Close();
                }
            }

            //return inn.newError(HTML);
            //Item createFile = inn.newItem("File", "add");
            //createFile.setProperty("filename", FileName);
            //createFile.attachPhysicalFile(path);

            //Item createFileRes = createFile.apply();
            ////CCO.Utilities.WriteDebug("List the Last Modified Elements.html", " HTML --> " + HTML);

            if (File.Exists(path))
            {
                //  File.Delete(path);
            }
            //return inn.newResult(createFileRes.getID());
            return path;
        }

    }
}
