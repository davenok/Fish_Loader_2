using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;


namespace FishLoader2
{
    class Program

    {
        static void Main(string[] args)
        {
            // Initialize variables
            int count = 0;
            int newrecords = 0;
            int updatedrecords = 0;


            // FISHBOWL - CONNECTION string using the firebirdsql client.
            // TODO: Move the config settings for this to config file.
            var csb = new FbConnectionStringBuilder();
            csb.Database = "";
            csb.DataSource = "";
            csb.Port = 3050;
            csb.UserID = "";
            csb.Password = "";
            csb.ServerType = FbServerType.Default;
            FbConnection fbconn = new FbConnection(csb.ToString());

            //  WAREHOUSE(37) - MySQL CONNECTION to using MySql client.
            // TODO: Move these settings to config file.
            string MySqlServer = "";
            string MySqlDatabase = "";
            string MySqlUser = "";
            string MySqlPassword = "";
            string connectionString;
            connectionString = "SERVER=" + MySqlServer + ";" + "DATABASE=" + MySqlDatabase + ";" + "UID=" + MySqlUser + ";" + "PASSWORD=" + MySqlPassword + ";";
            MySqlConnection MySqlconn = new MySqlConnection(connectionString);

            // Get latest record in us_so table. This function checks for missing table/empty table and handles.
            // ***** START DOING SOMETHING USEFUL *****

            // Get date of latest record from warehouse. Will insert table if doesn't exist.
            DateTime moddate = CheckSOTable(MySqlconn);
            Console.WriteLine("Max SO last modified date: " + moddate);

            // SELECT query to pull data from FishBowl SO Table
            string SQL = @"SELECT ID, STATUSID, CARRIERID, FOBPOINTID, PAYMENTTERMSID, COALESCE(BILLTOCOUNTRYID,0) AS BILLTOCOUNTRYID, COALESCE(BILLTOSTATEID,0) AS BILLTOSTATEID
                            , SHIPTERMSID, COALESCE(SHIPTOCOUNTRYID,0) AS SHIPTOCOUNTRYID, COALESCE(SHIPTOSTATEID,0) AS SHIPTOSTATEID, TYPEID
                            , CUSTOMERID, COALESCE(USERNAME,'gone') AS USERNAME, NUM, COALESCE(CUSTOMERPO,0) AS CUSTOMERPO
                            , COALESCE(VENDORPO,0) AS VENDORPO, CUSTOMERCONTACT , BILLTONAME, BILLTOADDRESS, BILLTOCITY
                            , BILLTOZIP, SALESMAN, SALESMANINITIALS, SHIPTONAME, SHIPTOADDRESS , SHIPTOCITY, SHIPTOZIP, RESIDENTIALFLAG
                            , COALESCE(REVISIONNUM,0) AS REVISIONNUM, DATELASTMODIFIED, DATECREATED, COALESCE(DATEISSUED,'2000-01-01') AS DATEISSUED
                            , COALESCE(DATEREVISION,'2000-01-01') AS DATEREVISION, DATEFIRSTSHIP, COALESCE(DATECOMPLETED,'2000-01-01') AS DATECOMPLETED
                            , TAXRATEID, TAXRATENAME, TAXRATE, TOTALTAX, QBCLASSID, LOCATIONGROUPID, NOTE, COST
                            , COALESCE(REGISTERID,0) AS REGISTERID, URL, TOTALINCLUDESTAX, COALESCE(UPSSERVICEID,0) AS UPSSERVICEID, PRIORITYID
                            , COALESCE(CURRENCYID,0) AS CURRENCYID, CURRENCYRATE, COALESCE(MCTOTALTAX,0) AS MCTOTALTAX, SALESMANID, TOBEPRINTED, TOBEEMAILED 
                            FROM SO WHERE DATELASTMODIFIED > @lastdate ORDER BY DATELASTMODIFIED ASC";
            // open fishbowl and MySql warehouse connection.
            MySqlconn.Open();
            fbconn.Open();
            FbCommand getSOcommand = new FbCommand(SQL, fbconn);
            getSOcommand.Parameters.AddWithValue("@lastdate", moddate);
            FbDataReader myReader = getSOcommand.ExecuteReader();

            while (myReader.Read())
            {
                // Console.WriteLine(myReader[0]);
                SORecord mysoRecord = new SORecord();
                mysoRecord.fbid = Convert.ToInt32(myReader["id"]);
                mysoRecord.statusid = Convert.ToInt32(myReader["statusid"]);
                mysoRecord.carrierid = Convert.ToInt32(myReader["carrierid"]);
                mysoRecord.fobpointid = Convert.ToInt32(myReader["fobpointid"]);
                mysoRecord.paymenttermsid = Convert.ToInt32(myReader["paymenttermsid"]);
                mysoRecord.billtocountryid = Convert.ToInt32(myReader["billtocountryid"]);
                mysoRecord.billtostateid = Convert.ToInt32(myReader["billtostateid"]);
                mysoRecord.shiptermsid = Convert.ToInt32(myReader["shiptermsid"]);
                mysoRecord.shiptocountryid = Convert.ToInt32(myReader["shiptocountryid"]);
                mysoRecord.shiptostateid = Convert.ToInt32(myReader["shiptostateid"]);
                mysoRecord.typeid = Convert.ToInt32(myReader["typeid"]);
                mysoRecord.customerid = Convert.ToInt32(myReader["customerid"]);
                mysoRecord.username = Convert.ToString(myReader["username"]);
                mysoRecord.num = Convert.ToString(myReader["num"]);
                mysoRecord.customerpo = Convert.ToString(myReader["customerpo"]);
                mysoRecord.vendorpo = Convert.ToString(myReader["vendorpo"]);
                mysoRecord.customerpo = Convert.ToString(myReader["customerpo"]);
                mysoRecord.billtoname = Convert.ToString(myReader["billtoname"]);
                mysoRecord.billtoaddress = Convert.ToString(myReader["billtoaddress"]);
                mysoRecord.billtocity = Convert.ToString(myReader["billtocity"]);
                mysoRecord.billtozip = Convert.ToString(myReader["billtozip"]);
                mysoRecord.salesman = Convert.ToString(myReader["salesman"]);
                mysoRecord.salesmaninitials = Convert.ToString(myReader["salesmaninitials"]);
                mysoRecord.shiptoname = Convert.ToString(myReader["shiptoname"]);
                mysoRecord.shiptoaddress = Convert.ToString(myReader["shiptoaddress"]);
                mysoRecord.shiptocity = Convert.ToString(myReader["shiptocity"]);
                mysoRecord.shiptozip = Convert.ToString(myReader["shiptozip"]);
                mysoRecord.residentialflag = Convert.ToString(myReader["residentialflag"]);
                mysoRecord.revisionnum = Convert.ToString(myReader["revisionnum"]);
                mysoRecord.datelastmodified = Convert.ToDateTime(myReader["datelastmodified"]);
                mysoRecord.datecreated = Convert.ToDateTime(myReader["datecreated"]);
                mysoRecord.dateissued = Convert.ToDateTime(myReader["dateissued"]);
                if (myReader["daterevision"] != null && myReader["daterevision"].ToString() != "")  {
                    mysoRecord.daterevision = Convert.ToDateTime(myReader["daterevision"]);
                } else
                {
                    mysoRecord.daterevision = new DateTime(2000, 01, 01);
                }
                mysoRecord.datefirstship = Convert.ToDateTime(myReader["datefirstship"]);
                mysoRecord.datecompleted = Convert.ToDateTime(myReader["datecompleted"]);
                mysoRecord.taxrateid = Convert.ToInt32(myReader["taxrateid"]);
                mysoRecord.taxratename = Convert.ToString(myReader["taxratename"]);
                mysoRecord.taxrate = Convert.ToDouble(myReader["taxrate"]);
                mysoRecord.totaltax = Convert.ToDouble(myReader["totaltax"]);
                mysoRecord.qbclassid = Convert.ToInt32(myReader["qbclassid"]);
                mysoRecord.locationgroupid = Convert.ToInt32(myReader["locationgroupid"]);
                mysoRecord.note = Convert.ToString(myReader["note"]);
                mysoRecord.cost = Convert.ToDouble(myReader["cost"]);
                if (myReader["registerid"] != null && myReader["registerid"].ToString() != "") {
                    mysoRecord.registerid = Convert.ToInt32(myReader["registerid"]);
                }
                mysoRecord.url = Convert.ToString(myReader["url"]);
                mysoRecord.totalincludestax = Convert.ToInt32(myReader["totalincludestax"]);
                if (myReader["upsserviceid"] != null && myReader["upsserviceid"].ToString() != "") {
                    mysoRecord.upsserviceid = Convert.ToInt32(myReader["upsserviceid"]);
                }
                mysoRecord.priorityid = Convert.ToInt32(myReader["priorityid"]);
                if (myReader["currencyid"] != null && myReader["currencyid"].ToString() != "") {
                    mysoRecord.currencyid = Convert.ToInt32(myReader["currencyid"]);
                }
                mysoRecord.currencyrate = Convert.ToDouble(myReader["currencyrate"]);
                if (myReader["mctotaltax"] != null && myReader["mctotaltax"].ToString() != ""){
                    mysoRecord.mctotaltax = Convert.ToDouble(myReader["mctotaltax"]);
                }
                mysoRecord.salesmanid = Convert.ToInt32(myReader["salesmanid"]);
                mysoRecord.tobeprinted = Convert.ToInt32(myReader["tobeprinted"]);
                mysoRecord.tobeemailed = Convert.ToInt32(myReader["tobeemailed"]);

                // query WH to see if record is new or update
                MySqlCommand cmd = MySqlconn.CreateCommand();
                cmd.CommandText = "SELECT id FROM us_so where fbid = @foo";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@foo", mysoRecord.fbid);
                MySqlDataReader reader = cmd.ExecuteReader();
                string query = "";
                int id = 0;
                if (reader.HasRows)
                {
                    reader.Read();
                    id = Convert.ToInt32(reader.GetValue(0));
                    
                    query = "UPDATE us_so SET ";
                    query += "FBID=@FBID, STATUSID=@STATUSID, CARRIERID=@CARRIERID, FOBPOINTID=@FOBPOINTID, PAYMENTTERMSID=@PAYMENTTERMSID ";
                    query += ", BILLTOCOUNTRYID=@BILLTOCOUNTRYID, BILLTOSTATEID=@BILLTOSTATEID, SHIPTERMSID=@SHIPTERMSID, SHIPTOCOUNTRYID=@SHIPTOCOUNTRYID";
                    query += ", SHIPTOCOUNTRYID=@SHIPTOCOUNTRYID, SHIPTOSTATEID=@SHIPTOSTATEID, TYPEID=@TYPEID, CUSTOMERID=@CUSTOMERID, USERNAME=@USERNAME";
                    query += ", NUM=@NUM, CUSTOMERPO=@CUSTOMERPO, VENDORPO=@VENDORPO, CUSTOMERCONTACT=@CUSTOMERCONTACT , BILLTONAME=@BILLTONAME";
                    query += ", BILLTOADDRESS=@BILLTOADDRESS, BILLTOCITY=@BILLTOCITY, BILLTOZIP=@BILLTOZIP, SALESMAN=@SALESMAN, SALESMANINITIALS=@SALESMANINITIALS";
                    query += ", SHIPTONAME=@SHIPTONAME, SHIPTOADDRESS=@SHIPTOADDRESS, SHIPTOCITY=@SHIPTOCITY, SHIPTOZIP=@SHIPTOZIP, RESIDENTIALFLAG=@RESIDENTIALFLAG";
                    query += ", REVISIONNUM=@REVISIONNUM, DATELASTMODIFIED=@DATELASTMODIFIED, DATECREATED=@DATECREATED, DATEISSUED=@DATEISSUED";
                    query += ", DATEREVISION=@DATEREVISION, DATEFIRSTSHIP=@DATEFIRSTSHIP, DATECOMPLETED=@DATECOMPLETED, TAXRATEID=@TAXRATEID";
                    query += ", TAXRATENAME=@TAXRATENAME, TAXRATE=@TAXRATE, TOTALTAX=@TOTALTAX, QBCLASSID=@QBCLASSID, LOCATIONGROUPID=@LOCATIONGROUPID";
                    query += ", NOTE=@NOTE, COST=@COST, REGISTERID=@REGISTERID, URL=@URL, TOTALINCLUDESTAX=@TOTALINCLUDESTAX, UPSSERVICEID=@UPSSERVICEID";
                    query += ", PRIORITYID=@PRIORITYID, CURRENCYID=@CURRENCYID, CURRENCYRATE=@CURRENCYRATE, MCTOTALTAX=@MCTOTALTAX ";
                    query += ", SALESMANID=@SALESMANID, TOBEPRINTED=@TOBEPRINTED, TOBEEMAILED=@TOBEEMAILED ";
                    query += "WHERE id=@ID";
                    updatedrecords++;
                }
                else
                {
                    query = "INSERT INTO us_so ";
                    query += "(FBID, STATUSID, CARRIERID, FOBPOINTID, PAYMENTTERMSID, BILLTOCOUNTRYID, BILLTOSTATEID, SHIPTERMSID ";
                    query += ", SHIPTOCOUNTRYID, SHIPTOSTATEID, TYPEID, CUSTOMERID, USERNAME, NUM, CUSTOMERPO, VENDORPO, CUSTOMERCONTACT ";
                    query += ", BILLTONAME, BILLTOADDRESS, BILLTOCITY, BILLTOZIP, SALESMAN, SALESMANINITIALS, SHIPTONAME, SHIPTOADDRESS ";
                    query += ", SHIPTOCITY, SHIPTOZIP, RESIDENTIALFLAG, REVISIONNUM, DATELASTMODIFIED, DATECREATED, DATEISSUED, DATEREVISION ";
                    query += ", DATEFIRSTSHIP, DATECOMPLETED, TAXRATEID, TAXRATENAME, TAXRATE, TOTALTAX, QBCLASSID, LOCATIONGROUPID ";
                    query += ", NOTE, COST, REGISTERID, URL, TOTALINCLUDESTAX, UPSSERVICEID, PRIORITYID, CURRENCYID, CURRENCYRATE ";
                    query += ", MCTOTALTAX, SALESMANID, TOBEPRINTED, TOBEEMAILED) ";
                    query += "VALUES ";
                    query += "(@FBID, @STATUSID, @CARRIERID, @FOBPOINTID, @PAYMENTTERMSID, @BILLTOCOUNTRYID, @BILLTOSTATEID, @SHIPTERMSID ";
                    query += ", @SHIPTOCOUNTRYID, @SHIPTOSTATEID, @TYPEID, @CUSTOMERID, @USERNAME, @NUM, @CUSTOMERPO, @VENDORPO, @CUSTOMERCONTACT ";
                    query += ", @BILLTONAME, @BILLTOADDRESS, @BILLTOCITY, @BILLTOZIP, @SALESMAN, @SALESMANINITIALS, @SHIPTONAME, @SHIPTOADDRESS ";
                    query += ", @SHIPTOCITY, @SHIPTOZIP, @RESIDENTIALFLAG,  @REVISIONNUM, @DATELASTMODIFIED, @DATECREATED, @DATEISSUED, @DATEREVISION ";
                    query += ", @DATEFIRSTSHIP, @DATECOMPLETED, @TAXRATEID, @TAXRATENAME, @TAXRATE, @TOTALTAX, @QBCLASSID, @LOCATIONGROUPID ";
                    query += ", @NOTE, @COST, @REGISTERID, @URL, @TOTALINCLUDESTAX, @UPSSERVICEID, @PRIORITYID, @CURRENCYID, @CURRENCYRATE ";
                    query += ", @MCTOTALTAX, @SALESMANID, @TOBEPRINTED, @TOBEEMAILED)";
                    newrecords++;
                }
                //create command and assign the query and connection from the constructor
                // MySqlCommand cmd = new MySqlCommand(query, MySqlconn);
                count++;
                reader.Close();
                
                cmd.CommandText = query;
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@fbid", mysoRecord.fbid);
                cmd.Parameters.AddWithValue("@statusid", mysoRecord.statusid);
                cmd.Parameters.AddWithValue("@carrierid", mysoRecord.carrierid);
                cmd.Parameters.AddWithValue("@fobpointid", mysoRecord.fobpointid);
                cmd.Parameters.AddWithValue("@paymenttermsid", mysoRecord.paymenttermsid);
                cmd.Parameters.AddWithValue("@billtocountryid", mysoRecord.billtocountryid);
                cmd.Parameters.AddWithValue("@shiptermsid", mysoRecord.shiptermsid);
                cmd.Parameters.AddWithValue("@shiptocountryid", mysoRecord.shiptocountryid);
                cmd.Parameters.AddWithValue("@shiptostateid", mysoRecord.shiptostateid);
                cmd.Parameters.AddWithValue("@typeid", mysoRecord.typeid);
                cmd.Parameters.AddWithValue("@customerid", mysoRecord.customerid);
                cmd.Parameters.AddWithValue("@username", mysoRecord.username);
                cmd.Parameters.AddWithValue("@num", mysoRecord.num);
                cmd.Parameters.AddWithValue("@customerpo", mysoRecord.customerpo);
                cmd.Parameters.AddWithValue("@vendorpo", mysoRecord.vendorpo);
                cmd.Parameters.AddWithValue("@customercontact", mysoRecord.customercontact);
                cmd.Parameters.AddWithValue("@billtoname", mysoRecord.billtoname);
                cmd.Parameters.AddWithValue("@billtoaddress", mysoRecord.billtoaddress);
                cmd.Parameters.AddWithValue("@billtocity", mysoRecord.billtocity);
                cmd.Parameters.AddWithValue("@billtostateid", mysoRecord.billtostateid);
                cmd.Parameters.AddWithValue("@billtozip", mysoRecord.billtozip);
                cmd.Parameters.AddWithValue("@salesman", mysoRecord.salesman);
                cmd.Parameters.AddWithValue("@salesmaninitials", mysoRecord.salesmaninitials);
                cmd.Parameters.AddWithValue("@shiptoname", mysoRecord.shiptoname);
                cmd.Parameters.AddWithValue("@shiptoaddress", mysoRecord.shiptoaddress);
                cmd.Parameters.AddWithValue("@shiptocity", mysoRecord.shiptocity);
                cmd.Parameters.AddWithValue("@shiptozip", mysoRecord.shiptozip);
                cmd.Parameters.AddWithValue("@residentialflag", mysoRecord.residentialflag);
                cmd.Parameters.AddWithValue("@revisionnum", mysoRecord.revisionnum);
                cmd.Parameters.AddWithValue("@datelastmodified", mysoRecord.datelastmodified);
                cmd.Parameters.AddWithValue("@datecreated", mysoRecord.datecreated);
                cmd.Parameters.AddWithValue("@dateissued", mysoRecord.dateissued);
                cmd.Parameters.AddWithValue("@daterevision", mysoRecord.daterevision);
                cmd.Parameters.AddWithValue("@datefirstship", mysoRecord.datefirstship);
                cmd.Parameters.AddWithValue("@datecompleted", mysoRecord.datecompleted);
                cmd.Parameters.AddWithValue("@taxrateid", mysoRecord.taxrateid);
                cmd.Parameters.AddWithValue("@taxratename", mysoRecord.taxratename);
                cmd.Parameters.AddWithValue("@taxrate", mysoRecord.taxrate);
                cmd.Parameters.AddWithValue("@totaltax", mysoRecord.totaltax);
                cmd.Parameters.AddWithValue("@qbclassid", mysoRecord.qbclassid);
                cmd.Parameters.AddWithValue("@locationgroupid", mysoRecord.locationgroupid);
                cmd.Parameters.AddWithValue("@note", mysoRecord.note);
                cmd.Parameters.AddWithValue("@cost", mysoRecord.cost);
                cmd.Parameters.AddWithValue("@registerid", mysoRecord.registerid);
                cmd.Parameters.AddWithValue("@url", mysoRecord.url);
                cmd.Parameters.AddWithValue("@totalincludestax", mysoRecord.totalincludestax);
                cmd.Parameters.AddWithValue("@upsserviceid", mysoRecord.upsserviceid);
                cmd.Parameters.AddWithValue("@priorityid", mysoRecord.priorityid);
                cmd.Parameters.AddWithValue("@currencyid", mysoRecord.currencyid);
                cmd.Parameters.AddWithValue("@currencyrate", mysoRecord.currencyrate);
                cmd.Parameters.AddWithValue("@mctotaltax", mysoRecord.mctotaltax);
                cmd.Parameters.AddWithValue("@salesmanid", mysoRecord.salesmanid);
                cmd.Parameters.AddWithValue("@tobeprinted", mysoRecord.tobeprinted);
                cmd.Parameters.AddWithValue("@tobeemailed", mysoRecord.tobeemailed);
                if (id != 0) { cmd.Parameters.AddWithValue("@id", id); }
                //Execute command
                cmd.ExecuteNonQuery();
                Console.Write(mysoRecord.num + " ");
            }
            fbconn.Close();
            MySqlconn.Close();

            Console.WriteLine("\nTotal SO Header Records processed: " + count + ". " + newrecords + " new records added. " + updatedrecords + " records updated.");


            /*
             * Done with SO headers. Time to get the SO Item lines
             */
            count = 0;
            newrecords = 0;
            updatedrecords = 0;

            DateTime soItemModDate = CheckSOItemTable(MySqlconn);







            Console.WriteLine("Max SO Line last modified date: " + soItemModDate);

            // SELECT query to pull data from FishBowl SO Table
            SQL = @"SELECT ID, PRODUCTID, SOID, TYPEID, STATUSID, UOMID, SOLINEITEM, DESCRIPTION, PRODUCTNUM, CUSTOMERPARTNUM, TAXABLEFLAG
                    , COALESCE(TAXID,0) AS TAXID, TAXRATE, QTYTOFULFILL, QTYFULFILLED, QTYPICKED, UNITPRICE, TOTALPRICE
                    , COALESCE(DATELASTFULFILLMENT,'2000-01-01') AS DATELASTFULFILLMENT
                    , COALESCE(DATESCHEDULEDFULFILLMENT, '2000-01-01') AS DATESCHEDULEDFULFILLMENT, REVLEVEL
                    , COALESCE(EXCHANGESOLINEITEM,0) AS EXCHANGESOLINEITEM, ADJUSTAMOUNT , ADJUSTPERCENTAGE, QBCLASSID, NOTE, TOTALCOST
                    , SHOWITEMFLAG, COALESCE(ITEMADJUSTID,0) AS ITEMADJUSTID, COALESCE(DATELASTMODIFIED,'2000-01-01') AS DATELASTMODIFIED
                    , COALESCE(MCTOTALPRICE,0) AS MCTOTALPRICE, MARKUPCOST FROM SOITEM 
                    WHERE DATELASTMODIFIED > @lastdate ORDER BY DATELASTMODIFIED ASC";
            // open fishbowl and MySql warehouse connection.
            MySqlconn.Open();
            fbconn.Open();
            FbCommand getSOIcommand = new FbCommand(SQL, fbconn);
            getSOcommand.Parameters.AddWithValue("@lastdate", soItemModDate);
            myReader = getSOIcommand.ExecuteReader();

            while (myReader.Read())
            {
                // Console.WriteLine(myReader[0]);
                // SORecord mysoRecord = new SORecord();
                SOItemRecord mySOI = new SOItemRecord();

                mySOI.FBId = Convert.ToInt32(myReader["id"]);


                mysoRecord.statusid = Convert.ToInt32(myReader["statusid"]);
                mysoRecord.carrierid = Convert.ToInt32(myReader["carrierid"]);
                mysoRecord.fobpointid = Convert.ToInt32(myReader["fobpointid"]);
                mysoRecord.paymenttermsid = Convert.ToInt32(myReader["paymenttermsid"]);
                mysoRecord.billtocountryid = Convert.ToInt32(myReader["billtocountryid"]);
                mysoRecord.billtostateid = Convert.ToInt32(myReader["billtostateid"]);
                mysoRecord.shiptermsid = Convert.ToInt32(myReader["shiptermsid"]);
                mysoRecord.shiptocountryid = Convert.ToInt32(myReader["shiptocountryid"]);
                mysoRecord.shiptostateid = Convert.ToInt32(myReader["shiptostateid"]);
                mysoRecord.typeid = Convert.ToInt32(myReader["typeid"]);
                mysoRecord.customerid = Convert.ToInt32(myReader["customerid"]);
                mysoRecord.username = Convert.ToString(myReader["username"]);
                mysoRecord.num = Convert.ToString(myReader["num"]);
                mysoRecord.customerpo = Convert.ToString(myReader["customerpo"]);
                mysoRecord.vendorpo = Convert.ToString(myReader["vendorpo"]);
                mysoRecord.customerpo = Convert.ToString(myReader["customerpo"]);
                mysoRecord.billtoname = Convert.ToString(myReader["billtoname"]);
                mysoRecord.billtoaddress = Convert.ToString(myReader["billtoaddress"]);
                mysoRecord.billtocity = Convert.ToString(myReader["billtocity"]);
                mysoRecord.billtozip = Convert.ToString(myReader["billtozip"]);
                mysoRecord.salesman = Convert.ToString(myReader["salesman"]);
                mysoRecord.salesmaninitials = Convert.ToString(myReader["salesmaninitials"]);
                mysoRecord.shiptoname = Convert.ToString(myReader["shiptoname"]);
                mysoRecord.shiptoaddress = Convert.ToString(myReader["shiptoaddress"]);
                mysoRecord.shiptocity = Convert.ToString(myReader["shiptocity"]);
                mysoRecord.shiptozip = Convert.ToString(myReader["shiptozip"]);
                mysoRecord.residentialflag = Convert.ToString(myReader["residentialflag"]);
                mysoRecord.revisionnum = Convert.ToString(myReader["revisionnum"]);
                mysoRecord.datelastmodified = Convert.ToDateTime(myReader["datelastmodified"]);
                mysoRecord.datecreated = Convert.ToDateTime(myReader["datecreated"]);
                mysoRecord.dateissued = Convert.ToDateTime(myReader["dateissued"]);
                if (myReader["daterevision"] != null && myReader["daterevision"].ToString() != "")
                {
                    mysoRecord.daterevision = Convert.ToDateTime(myReader["daterevision"]);
                }
                else
                {
                    mysoRecord.daterevision = new DateTime(2000, 01, 01);
                }
                mysoRecord.datefirstship = Convert.ToDateTime(myReader["datefirstship"]);
                mysoRecord.datecompleted = Convert.ToDateTime(myReader["datecompleted"]);
                mysoRecord.taxrateid = Convert.ToInt32(myReader["taxrateid"]);
                mysoRecord.taxratename = Convert.ToString(myReader["taxratename"]);
                mysoRecord.taxrate = Convert.ToDouble(myReader["taxrate"]);
                mysoRecord.totaltax = Convert.ToDouble(myReader["totaltax"]);
                mysoRecord.qbclassid = Convert.ToInt32(myReader["qbclassid"]);
                mysoRecord.locationgroupid = Convert.ToInt32(myReader["locationgroupid"]);
                mysoRecord.note = Convert.ToString(myReader["note"]);
                mysoRecord.cost = Convert.ToDouble(myReader["cost"]);
                if (myReader["registerid"] != null && myReader["registerid"].ToString() != "")
                {
                    mysoRecord.registerid = Convert.ToInt32(myReader["registerid"]);
                }
                mysoRecord.url = Convert.ToString(myReader["url"]);
                mysoRecord.totalincludestax = Convert.ToInt32(myReader["totalincludestax"]);
                if (myReader["upsserviceid"] != null && myReader["upsserviceid"].ToString() != "")
                {
                    mysoRecord.upsserviceid = Convert.ToInt32(myReader["upsserviceid"]);
                }
                mysoRecord.priorityid = Convert.ToInt32(myReader["priorityid"]);
                if (myReader["currencyid"] != null && myReader["currencyid"].ToString() != "")
                {
                    mysoRecord.currencyid = Convert.ToInt32(myReader["currencyid"]);
                }
                mysoRecord.currencyrate = Convert.ToDouble(myReader["currencyrate"]);
                if (myReader["mctotaltax"] != null && myReader["mctotaltax"].ToString() != "")
                {
                    mysoRecord.mctotaltax = Convert.ToDouble(myReader["mctotaltax"]);
                }
                mysoRecord.salesmanid = Convert.ToInt32(myReader["salesmanid"]);
                mysoRecord.tobeprinted = Convert.ToInt32(myReader["tobeprinted"]);
                mysoRecord.tobeemailed = Convert.ToInt32(myReader["tobeemailed"]);

                // query WH to see if record is new or update
                MySqlCommand cmd = MySqlconn.CreateCommand();
                cmd.CommandText = "SELECT id FROM us_so where fbid = @foo";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@foo", mysoRecord.fbid);
                MySqlDataReader reader = cmd.ExecuteReader();
                string query = "";
                int id = 0;
                if (reader.HasRows)
                {
                    reader.Read();
                    id = Convert.ToInt32(reader.GetValue(0));

                    query = "UPDATE us_so SET ";
                    query += "FBID=@FBID, STATUSID=@STATUSID, CARRIERID=@CARRIERID, FOBPOINTID=@FOBPOINTID, PAYMENTTERMSID=@PAYMENTTERMSID ";
                    query += ", BILLTOCOUNTRYID=@BILLTOCOUNTRYID, BILLTOSTATEID=@BILLTOSTATEID, SHIPTERMSID=@SHIPTERMSID, SHIPTOCOUNTRYID=@SHIPTOCOUNTRYID";
                    query += ", SHIPTOCOUNTRYID=@SHIPTOCOUNTRYID, SHIPTOSTATEID=@SHIPTOSTATEID, TYPEID=@TYPEID, CUSTOMERID=@CUSTOMERID, USERNAME=@USERNAME";
                    query += ", NUM=@NUM, CUSTOMERPO=@CUSTOMERPO, VENDORPO=@VENDORPO, CUSTOMERCONTACT=@CUSTOMERCONTACT , BILLTONAME=@BILLTONAME";
                    query += ", BILLTOADDRESS=@BILLTOADDRESS, BILLTOCITY=@BILLTOCITY, BILLTOZIP=@BILLTOZIP, SALESMAN=@SALESMAN, SALESMANINITIALS=@SALESMANINITIALS";
                    query += ", SHIPTONAME=@SHIPTONAME, SHIPTOADDRESS=@SHIPTOADDRESS, SHIPTOCITY=@SHIPTOCITY, SHIPTOZIP=@SHIPTOZIP, RESIDENTIALFLAG=@RESIDENTIALFLAG";
                    query += ", REVISIONNUM=@REVISIONNUM, DATELASTMODIFIED=@DATELASTMODIFIED, DATECREATED=@DATECREATED, DATEISSUED=@DATEISSUED";
                    query += ", DATEREVISION=@DATEREVISION, DATEFIRSTSHIP=@DATEFIRSTSHIP, DATECOMPLETED=@DATECOMPLETED, TAXRATEID=@TAXRATEID";
                    query += ", TAXRATENAME=@TAXRATENAME, TAXRATE=@TAXRATE, TOTALTAX=@TOTALTAX, QBCLASSID=@QBCLASSID, LOCATIONGROUPID=@LOCATIONGROUPID";
                    query += ", NOTE=@NOTE, COST=@COST, REGISTERID=@REGISTERID, URL=@URL, TOTALINCLUDESTAX=@TOTALINCLUDESTAX, UPSSERVICEID=@UPSSERVICEID";
                    query += ", PRIORITYID=@PRIORITYID, CURRENCYID=@CURRENCYID, CURRENCYRATE=@CURRENCYRATE, MCTOTALTAX=@MCTOTALTAX ";
                    query += ", SALESMANID=@SALESMANID, TOBEPRINTED=@TOBEPRINTED, TOBEEMAILED=@TOBEEMAILED ";
                    query += "WHERE id=@ID";
                    updatedrecords++;
                }
                else
                {
                    query = "INSERT INTO us_so ";
                    query += "(FBID, STATUSID, CARRIERID, FOBPOINTID, PAYMENTTERMSID, BILLTOCOUNTRYID, BILLTOSTATEID, SHIPTERMSID ";
                    query += ", SHIPTOCOUNTRYID, SHIPTOSTATEID, TYPEID, CUSTOMERID, USERNAME, NUM, CUSTOMERPO, VENDORPO, CUSTOMERCONTACT ";
                    query += ", BILLTONAME, BILLTOADDRESS, BILLTOCITY, BILLTOZIP, SALESMAN, SALESMANINITIALS, SHIPTONAME, SHIPTOADDRESS ";
                    query += ", SHIPTOCITY, SHIPTOZIP, RESIDENTIALFLAG, REVISIONNUM, DATELASTMODIFIED, DATECREATED, DATEISSUED, DATEREVISION ";
                    query += ", DATEFIRSTSHIP, DATECOMPLETED, TAXRATEID, TAXRATENAME, TAXRATE, TOTALTAX, QBCLASSID, LOCATIONGROUPID ";
                    query += ", NOTE, COST, REGISTERID, URL, TOTALINCLUDESTAX, UPSSERVICEID, PRIORITYID, CURRENCYID, CURRENCYRATE ";
                    query += ", MCTOTALTAX, SALESMANID, TOBEPRINTED, TOBEEMAILED) ";
                    query += "VALUES ";
                    query += "(@FBID, @STATUSID, @CARRIERID, @FOBPOINTID, @PAYMENTTERMSID, @BILLTOCOUNTRYID, @BILLTOSTATEID, @SHIPTERMSID ";
                    query += ", @SHIPTOCOUNTRYID, @SHIPTOSTATEID, @TYPEID, @CUSTOMERID, @USERNAME, @NUM, @CUSTOMERPO, @VENDORPO, @CUSTOMERCONTACT ";
                    query += ", @BILLTONAME, @BILLTOADDRESS, @BILLTOCITY, @BILLTOZIP, @SALESMAN, @SALESMANINITIALS, @SHIPTONAME, @SHIPTOADDRESS ";
                    query += ", @SHIPTOCITY, @SHIPTOZIP, @RESIDENTIALFLAG,  @REVISIONNUM, @DATELASTMODIFIED, @DATECREATED, @DATEISSUED, @DATEREVISION ";
                    query += ", @DATEFIRSTSHIP, @DATECOMPLETED, @TAXRATEID, @TAXRATENAME, @TAXRATE, @TOTALTAX, @QBCLASSID, @LOCATIONGROUPID ";
                    query += ", @NOTE, @COST, @REGISTERID, @URL, @TOTALINCLUDESTAX, @UPSSERVICEID, @PRIORITYID, @CURRENCYID, @CURRENCYRATE ";
                    query += ", @MCTOTALTAX, @SALESMANID, @TOBEPRINTED, @TOBEEMAILED)";
                    newrecords++;
                }
                //create command and assign the query and connection from the constructor
                // MySqlCommand cmd = new MySqlCommand(query, MySqlconn);
                count++;
                reader.Close();

                cmd.CommandText = query;
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@fbid", mysoRecord.fbid);
                cmd.Parameters.AddWithValue("@statusid", mysoRecord.statusid);
                cmd.Parameters.AddWithValue("@carrierid", mysoRecord.carrierid);
                cmd.Parameters.AddWithValue("@fobpointid", mysoRecord.fobpointid);
                cmd.Parameters.AddWithValue("@paymenttermsid", mysoRecord.paymenttermsid);
                cmd.Parameters.AddWithValue("@billtocountryid", mysoRecord.billtocountryid);
                cmd.Parameters.AddWithValue("@shiptermsid", mysoRecord.shiptermsid);
                cmd.Parameters.AddWithValue("@shiptocountryid", mysoRecord.shiptocountryid);
                cmd.Parameters.AddWithValue("@shiptostateid", mysoRecord.shiptostateid);
                cmd.Parameters.AddWithValue("@typeid", mysoRecord.typeid);
                cmd.Parameters.AddWithValue("@customerid", mysoRecord.customerid);
                cmd.Parameters.AddWithValue("@username", mysoRecord.username);
                cmd.Parameters.AddWithValue("@num", mysoRecord.num);
                cmd.Parameters.AddWithValue("@customerpo", mysoRecord.customerpo);
                cmd.Parameters.AddWithValue("@vendorpo", mysoRecord.vendorpo);
                cmd.Parameters.AddWithValue("@customercontact", mysoRecord.customercontact);
                cmd.Parameters.AddWithValue("@billtoname", mysoRecord.billtoname);
                cmd.Parameters.AddWithValue("@billtoaddress", mysoRecord.billtoaddress);
                cmd.Parameters.AddWithValue("@billtocity", mysoRecord.billtocity);
                cmd.Parameters.AddWithValue("@billtostateid", mysoRecord.billtostateid);
                cmd.Parameters.AddWithValue("@billtozip", mysoRecord.billtozip);
                cmd.Parameters.AddWithValue("@salesman", mysoRecord.salesman);
                cmd.Parameters.AddWithValue("@salesmaninitials", mysoRecord.salesmaninitials);
                cmd.Parameters.AddWithValue("@shiptoname", mysoRecord.shiptoname);
                cmd.Parameters.AddWithValue("@shiptoaddress", mysoRecord.shiptoaddress);
                cmd.Parameters.AddWithValue("@shiptocity", mysoRecord.shiptocity);
                cmd.Parameters.AddWithValue("@shiptozip", mysoRecord.shiptozip);
                cmd.Parameters.AddWithValue("@residentialflag", mysoRecord.residentialflag);
                cmd.Parameters.AddWithValue("@revisionnum", mysoRecord.revisionnum);
                cmd.Parameters.AddWithValue("@datelastmodified", mysoRecord.datelastmodified);
                cmd.Parameters.AddWithValue("@datecreated", mysoRecord.datecreated);
                cmd.Parameters.AddWithValue("@dateissued", mysoRecord.dateissued);
                cmd.Parameters.AddWithValue("@daterevision", mysoRecord.daterevision);
                cmd.Parameters.AddWithValue("@datefirstship", mysoRecord.datefirstship);
                cmd.Parameters.AddWithValue("@datecompleted", mysoRecord.datecompleted);
                cmd.Parameters.AddWithValue("@taxrateid", mysoRecord.taxrateid);
                cmd.Parameters.AddWithValue("@taxratename", mysoRecord.taxratename);
                cmd.Parameters.AddWithValue("@taxrate", mysoRecord.taxrate);
                cmd.Parameters.AddWithValue("@totaltax", mysoRecord.totaltax);
                cmd.Parameters.AddWithValue("@qbclassid", mysoRecord.qbclassid);
                cmd.Parameters.AddWithValue("@locationgroupid", mysoRecord.locationgroupid);
                cmd.Parameters.AddWithValue("@note", mysoRecord.note);
                cmd.Parameters.AddWithValue("@cost", mysoRecord.cost);
                cmd.Parameters.AddWithValue("@registerid", mysoRecord.registerid);
                cmd.Parameters.AddWithValue("@url", mysoRecord.url);
                cmd.Parameters.AddWithValue("@totalincludestax", mysoRecord.totalincludestax);
                cmd.Parameters.AddWithValue("@upsserviceid", mysoRecord.upsserviceid);
                cmd.Parameters.AddWithValue("@priorityid", mysoRecord.priorityid);
                cmd.Parameters.AddWithValue("@currencyid", mysoRecord.currencyid);
                cmd.Parameters.AddWithValue("@currencyrate", mysoRecord.currencyrate);
                cmd.Parameters.AddWithValue("@mctotaltax", mysoRecord.mctotaltax);
                cmd.Parameters.AddWithValue("@salesmanid", mysoRecord.salesmanid);
                cmd.Parameters.AddWithValue("@tobeprinted", mysoRecord.tobeprinted);
                cmd.Parameters.AddWithValue("@tobeemailed", mysoRecord.tobeemailed);
                if (id != 0) { cmd.Parameters.AddWithValue("@id", id); }
                //Execute command
                cmd.ExecuteNonQuery();
                Console.Write(mysoRecord.num + " ");
            }
            fbconn.Close();
            MySqlconn.Close();

            Console.WriteLine("\nTotal SO Header Records processed: " + count + ". " + newrecords + " new records added. " + updatedrecords + " records updated.");












            fbconn.Close();
            MySqlconn.Close();
            Console.WriteLine("Press enter when done");
            Console.ReadLine();
        }



        static DateTime CheckSOTable(MySqlConnection MysqlConn)
        {
            // This checks if the table exists. If not, table is created.
            MySqlCommand query = new MySqlCommand("SHOW TABLES LIKE 'us_so'", MysqlConn);
            try
            {
                MysqlConn.Open();
                MySqlDataReader reader = query.ExecuteReader();
                if (reader.HasRows)
                {
                    // us_so table does exist. Find newest record and return it. 
                    // otherwise return ancient date so table is rebuilt from inception.
                    reader.Close();
                    query.CommandText = "SELECT MAX(datelastmodified) FROM us_so";
                    try
                    {
                        reader = query.ExecuteReader();
                        reader.Read();
                        string foo = Convert.ToString(reader.GetValue(0));
                        if (foo != "")
                        {
                            reader.Close();
                            MysqlConn.Close();
                            return Convert.ToDateTime(foo);
                        }
                        else
                        {
                            reader.Close();
                            MysqlConn.Close();
                            return new DateTime(2000, 01, 01);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    // table did not exist. Create it then return ancient date to build contents from inception.
                    reader.Close();
                    string sql = @"CREATE TABLE us_so 
                            (	ID INTEGER NOT NULL AUTO_INCREMENT, FBID INTEGER NOT NULL, STATUSID INTEGER NOT NULL, CARRIERID INTEGER NOT NULL, FOBPOINTID INTEGER DEFAULT 10 NOT NULL
                                , PAYMENTTERMSID INTEGER NOT NULL, BILLTOCOUNTRYID INTEGER, BILLTOSTATEID INTEGER, SHIPTERMSID INTEGER NOT NULL
                                , SHIPTOCOUNTRYID INTEGER, SHIPTOSTATEID INTEGER, TYPEID INTEGER NOT NULL, CUSTOMERID INTEGER NOT NULL, USERNAME VARCHAR(15) DEFAULT null
                                , NUM VARCHAR(25) NOT NULL, CUSTOMERPO VARCHAR(25) DEFAULT null, VENDORPO VARCHAR(25) DEFAULT null, CUSTOMERCONTACT VARCHAR(30) DEFAULT null
                                , BILLTONAME VARCHAR(41) DEFAULT null, BILLTOADDRESS VARCHAR(90) DEFAULT null, BILLTOCITY VARCHAR(30) DEFAULT null, BILLTOZIP VARCHAR(10) DEFAULT null
                                , SALESMAN VARCHAR(30) DEFAULT null, SALESMANINITIALS VARCHAR(5) DEFAULT null, SHIPTONAME VARCHAR(41) DEFAULT null, SHIPTOADDRESS VARCHAR(90) DEFAULT null
                                , SHIPTOCITY VARCHAR(30) DEFAULT null, SHIPTOZIP VARCHAR(10) DEFAULT null, RESIDENTIALFLAG SMALLINT DEFAULT 0, REVISIONNUM INTEGER DEFAULT 0
                                , DATELASTMODIFIED DATETIME DEFAULT null, DATECREATED DATETIME DEFAULT null , DATEISSUED DATETIME DEFAULT null
                                , DATEREVISION DATETIME DEFAULT null, DATEFIRSTSHIP DATETIME DEFAULT null, DATECOMPLETED DATETIME DEFAULT null
                                , TAXRATEID INTEGER, TAXRATENAME VARCHAR(31) DEFAULT 'None', TAXRATE DOUBLE PRECISION(15,0) DEFAULT 0
                                , TOTALTAX DOUBLE PRECISION(15,0), QBCLASSID INTEGER DEFAULT 1, LOCATIONGROUPID INTEGER NOT NULL, NOTE BLOB
                                , COST DOUBLE PRECISION(15,0) DEFAULT 0, REGISTERID INTEGER DEFAULT 0, URL VARCHAR(256) DEFAULT null
                                , TOTALINCLUDESTAX SMALLINT DEFAULT 0, UPSSERVICEID INTEGER, PRIORITYID INTEGER, CURRENCYID INTEGER
                                , CURRENCYRATE DOUBLE PRECISION(15,0) DEFAULT 1, MCTOTALTAX DOUBLE PRECISION(15,0) DEFAULT 0, SALESMANID INTEGER
                                , TOBEPRINTED SMALLINT DEFAULT 0 NOT NULL, TOBEEMAILED SMALLINT DEFAULT 0 NOT NULL
                                , PRIMARY KEY (ID) )";

                    query.Parameters.Clear();
                    query.CommandText = sql;
                    query.Connection = MysqlConn;
                    query.ExecuteNonQuery();
                    DateTime moddate = new DateTime(2000, 01, 01);
                    reader.Close();
                    MysqlConn.Close();
                    return moddate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            MysqlConn.Close();
            return new DateTime(2000, 01, 01);
            // End of CheckSOTable
        }


        static DateTime CheckSOItemTable(MySqlConnection MysqlConn)
        {
            // This checks if the table exists. If not, table is created.
            MySqlCommand query = new MySqlCommand("SHOW TABLES LIKE 'us_soitem'", MysqlConn);
            try
            {
                MysqlConn.Open();
                MySqlDataReader reader = query.ExecuteReader();
                if (reader.HasRows)
                {
                    // us_soitem table does exist. Find newest record and return it. 
                    // otherwise return ancient date so table is rebuilt from inception.
                    reader.Close();
                    query.CommandText = "SELECT MAX(datelastmodified) FROM us_soitem";
                    try
                    {
                        reader = query.ExecuteReader();
                        reader.Read();
                        string foo = Convert.ToString(reader.GetValue(0));
                        if (foo != "")
                        {
                            reader.Close();
                            MysqlConn.Close();
                            return Convert.ToDateTime(foo);
                        }
                        else
                        {
                            reader.Close();
                            MysqlConn.Close();
                            return new DateTime(2000, 01, 01);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    // table did not exist. Create it then return ancient date to build contents from inception.
                    reader.Close();
                    string sql = @"CREATE TABLE us_soitem ( ID INTEGER NOT NULL AUTO_INCREMENT
                                , FBID INTEGER NOT NULL, PRODUCTID INTEGER, SOID INTEGER NOT NULL, TYPEID INTEGER NOT NULL
                                , STATUSID INTEGER NOT NULL, UOMID INTEGER, SOLINEITEM INTEGER NOT NULL, DESCRIPTION VARCHAR(256) DEFAULT null
                                , PRODUCTNUM VARCHAR(70) NOT NULL, CUSTOMERPARTNUM VARCHAR(70) DEFAULT null, TAXABLEFLAG SMALLINT DEFAULT 0
                                , TAXID INTEGER, TAXRATE DOUBLE PRECISION(15,0), QTYTOFULFILL DOUBLE PRECISION(15,0) DEFAULT 0
                                , QTYFULFILLED DOUBLE PRECISION(15,0) DEFAULT 0, QTYPICKED DOUBLE PRECISION(15,0) DEFAULT 0
                                , UNITPRICE DOUBLE PRECISION(15,0), TOTALPRICE DOUBLE PRECISION(15,0), DATELASTFULFILLMENT DATETIME DEFAULT null
                                , DATESCHEDULEDFULFILLMENT DATETIME DEFAULT NULL, REVLEVEL VARCHAR(15) DEFAULT null
                                , EXCHANGESOLINEITEM INTEGER DEFAULT 0, ADJUSTAMOUNT DOUBLE PRECISION(15,0)
                                , ADJUSTPERCENTAGE DOUBLE PRECISION(15,0) DEFAULT 0, QBCLASSID INTEGER DEFAULT 1, NOTE BLOB
                                , TOTALCOST DOUBLE PRECISION(15,0) DEFAULT 0, SHOWITEMFLAG SMALLINT DEFAULT 1 NOT NULL
                                , ITEMADJUSTID INTEGER, DATELASTMODIFIED DATETIME DEFAULT null
                                , MCTOTALPRICE DOUBLE PRECISION(15,0) DEFAULT 0
                                , MARKUPCOST DOUBLE PRECISION(15,0) DEFAULT 0 NOT NULL
                                , PRIMARY KEY (ID))";

                    query.Parameters.Clear();
                    query.CommandText = sql;
                    query.Connection = MysqlConn;
                    query.ExecuteNonQuery();
                    DateTime moddate = new DateTime(2000, 01, 01);
                    reader.Close();
                    MysqlConn.Close();
                    return moddate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
            MysqlConn.Close();
            return new DateTime(2000, 01, 01);
            // End of CheckSOItemTable
        }








        public class SORecord
        {
            public int fbid;
            public int statusid;
            public int carrierid;
            public int fobpointid;
            public int paymenttermsid;
            public int billtocountryid;
            public int billtostateid;
            public int shiptermsid;
            public int shiptocountryid;
            public int shiptostateid;
            public int typeid;
            public int customerid;
            public string username;
            public string num;
            public string customerpo;
            public string vendorpo;
            public string customercontact;
            public string billtoname;
            public string billtoaddress;
            public string billtocity;
            public string billtozip;
            public string salesman;
            public string salesmaninitials;
            public string shiptoname;
            public string shiptoaddress;
            public string shiptocity;
            public string shiptozip;
            public string residentialflag;
            public string revisionnum;
            public DateTime datelastmodified;
            public DateTime datecreated;
            public DateTime dateissued;
            public DateTime daterevision;
            public DateTime datefirstship;
            public DateTime datecompleted;
            public int taxrateid;
            public string taxratename;
            public double taxrate;
            public double totaltax;
            public int qbclassid;
            public int locationgroupid;
            public string note;
            public double cost;
            public int registerid;
            public string url;
            public int totalincludestax;
            public int upsserviceid;
            public int priorityid;
            public int currencyid;
            public double currencyrate;
            public double mctotaltax;
            public int salesmanid;
            public int tobeprinted;
            public int tobeemailed;
        }







    }
}
