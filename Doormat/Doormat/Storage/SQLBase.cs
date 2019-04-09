
using DoormatCore.Games;
using DoormatCore.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using static DoormatCore.Sites.BaseSite;

namespace DoormatCore.Storage
{
    /// <summary>
    /// Base interface for reading and writing data to and from a database.
    /// </summary>
    abstract class SQLBase
    {
        public static SQLBase OpenConnection(string ConnectionString, string Provider)
        {
            SQLBase newbase = null;
            Logger.DumpLog("Creating SQLBase instance", 6);
            switch (Provider.ToLower())
            {
                case "mysql": newbase = new MYSql(ConnectionString);break ;
                case "sqlite": newbase = new SQLite(ConnectionString); break;
                case "mssql": newbase = new MSSQL(ConnectionString); break;
                case "mongo": newbase = new Mongo(ConnectionString); break;
                case "postgre": newbase = new PostGre(ConnectionString); break;
                default: newbase = new SQLite(ConnectionString); break;

            }
            Logger.DumpLog("Created SQLBase instance", 6);

            newbase.CreateTables();
            return newbase;
        }

        public static long DateToLong(DateTime DateValue)
        {
            return (long)(DateValue - new DateTime(1970, 1, 1)).TotalSeconds;
        }
        public static decimal DateToDecimal(DateTime DateValue)
        {
            return (decimal)(DateValue - new DateTime(1970, 1, 1)).TotalMilliseconds/1000.0m;
        }
        public static DateTime DateFromLong(long DateValue)
        {
            return new DateTime(1970, 1, 1).AddSeconds(DateValue);
        }
        public static DateTime DateFromDecimal(decimal DateValue)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds((double)(DateValue*1000.0m));
        }

        public string ProviderName { get; protected set; }
        public abstract string GetConnectionString();


        protected SQLBase(string ConnectionString)
        {
            
        }
        //save bet
        //get bet
        //get bets
        //get bets by date range
        //get bets from search
        //save seed
        //get seed
        //get seeds
        //get users
        //get user
        //add user



        //passwords?
        //save session stats

        //Create tables and insert default data

        public void CreateTables()
        {
            Logger.DumpLog("Creating Tables", 6);

            CreateCurrencyT();
            CreateSiteT();
            CreateUserT();
            CreateSeedT();
            CreateBetT();
             CreateLoginParamT();
            CreateSessionStatsT();
            Logger.DumpLog("Tables Created", 6);
        }

        #region Session Stats
        protected abstract void CreateSessionStatsT();
        public abstract SessionStats AddSessionStats(SessionStats Stats);
        public abstract SessionStats[] GetSessionStats();
        public abstract SessionStats[] GetSessionStats(Site Site);
        public abstract SessionStats[] GetSessionStats(User User);
        public virtual SessionStats GetSessionStatsParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<SessionStats>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }
        #endregion

        #region Currencies
        protected abstract void CreateCurrencyT();
        public abstract Currency UpdateCurrency(Currency NewCurrency);
        public abstract Currency GetCurrency(int Id);
        public abstract Currency GetCurrency(string Nameorsymbol);
        public abstract Currency[] GetCurrenciesForSite(Site Site);
        public virtual Currency CurrencyParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<Currency>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }





        #endregion

        #region Sites
        protected abstract void CreateSiteT();
        public abstract Site UpdateSite(Site NewSite);
        public abstract Site GetSite(int Id);
        public abstract Site GetSite(string SiteName);
        public abstract Site GetSites(bool ActiveOnly);
        public virtual Site SiteParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<Site>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }
        //Add Site
        //Get Site by id, name
        //Get Sites all, active
        //Site Parser
        #endregion

        #region User
        protected abstract void CreateUserT();
        public abstract User UpdateUser(User NewUser);
        public abstract User GetUser(int Id);
        public abstract User GetUserByName(string Name);
        public abstract User GetUserById(string Id);
        public virtual User UserParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<User>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }
        #endregion

        #region Seeds
        protected abstract void CreateSeedT();
        public abstract Seed UpdateSeed(Seed NewSeed);
        public abstract Seed GetSeed(int Id);
        public abstract Seed GetSeed(string ServerSeedHash, string ServerSeed = null);
        public abstract Seed[] GetSeeds();
        public virtual Seed SeedParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<Seed>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }
        #endregion

        #region Bets
        protected abstract void CreateBetT();
        public Bet UpdateBet(Bet NewBet)
        {
            if (NewBet is DiceBet)
            {
                return UpdateBet(NewBet as DiceBet);
            }
            if (NewBet is RouletteBet)
            {
                return UpdateBet(NewBet as RouletteBet);
            }
            if (NewBet is PlinkoBet)
            {
                return UpdateBet(NewBet as PlinkoBet);
            }
            if (NewBet is CrashBet)
            {
                return UpdateBet(NewBet as CrashBet);
            }
            return NewBet;
        }

        public abstract DiceBet UpdateBet(DiceBet NewBet);

        public abstract DiceBet GetBet(int InternalId);
        public abstract DiceBet GetBet(string BetId);
        public abstract DiceBet[] GetBets();
        public abstract DiceBet[] GetBets(Site Site);
        public abstract DiceBet[] GetBets(User User);
        public virtual DiceBet DiceBetParser(object Reader)
        {
            if (Reader is IDataReader)
            {
                //do stuff here
                return ParseResult<DiceBet>(Reader as IDataReader);
            }
            else
            {
                throw new Exception("Invalid Ojbect type. Please use an IDataReader for the default parsers");
            }
        }
        
        /*
         * add ways to query bets for exporting/grids/charts etc etc
         * public abstract DiceBet[] GetBets(Site Site);
         * public abstract DiceBet[] GetBets(Site Site);
        */
        #endregion

      

        #region LoginParams
        protected abstract void CreateLoginParamT();
        public abstract LoginParamValue UpdateValue(LoginParamValue NewValue);
        public abstract LoginParameter UpdateParameter(LoginParameter LoginParameter);
        public abstract LoginParameter GetParameter(int Id);


        #endregion

        protected T ParseResult<T>(IDataReader Reader) where T : new()
        {
            Logger.DumpLog("Parsing result for DB", 5);
            Type typ = typeof(T);
            T tmp = new T();
            foreach (PropertyInfo x in typ.GetProperties())
            {
                Logger.DumpLog($"Checking {x.Name}", 6);
                if (!Attribute.IsDefined(x, typeof(NonPersistent)) && !x.PropertyType.IsArray)
                {
                    Logger.DumpLog($"Found {x.Name}, getting index and checking null", 6);
                    int index = Reader.GetOrdinal(x.Name);
                    if (Reader.IsDBNull(index))
                    {
                        Logger.DumpLog($"Found {x.Name}, null", 6);
                    }
                    else
                    {
                        Logger.DumpLog($"Found {x.Name}, setting value", 6);
                        x.SetValue(tmp, Reader[x.Name]);
                    }
                }
            }
            return tmp;
        }



        //Sites
        //Site name, link, class name, image, currencies

        //currency
        //name, icon, abbr

        //password man
        //encrypted site, last updated
        //encrypted login parameters. NO 2fa ever

        //seeds
        //server seed
        //client seed
        //server seed hash index pk
        //nonce

        //user
        //site, user name, user id, 




        //saving type:
        /*
         * mongo
         * mssql
         * mysql
         * localdb
         * postgre
         * sqlite
         * csv - bets only 
         * excel - bets only
         * 
         * */

    }
    [PersistentTableName("CURRENCY")]
    public class Currency : PersistentBase
    {
        
        public string Name { get; set; }
        public string Symbol { get; set; }
        public byte[] Icon { get; set; }
    }
    [PersistentTableName("SEED")]
    public class Seed : PersistentBase
    {
        
        public string ServerSeed { get; set; }
        public string ServerSeedHash { get; set; }
        public string ClientSeed { get; set; }
        public long Nonce { get; set; }
    }
    [PersistentTableName("USER")]
    public class User : PersistentBase
    {
        
        public Site Site { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
    }
    [PersistentTableName("SITE")]
    public class Site : PersistentBase
    {
        //Site name, link, class name, image, currencies
        
        public string Name { get; set; }
        public string Link { get; set; }
        public string ClassName { get; set; }
        [NonPersistent]
        public byte[] Image { get; set; }
        public Currency[] Currencies { get; set; }
    }

    
    public abstract class PersistentBase
    {
        public int Id { get; set; }
    }
    public class PersistentTableName:Attribute
    {
        public string TableName { get; set; }
        public PersistentTableName(string TableName)
        {
            this.TableName = TableName;
        }
    }
    public class NonPersistent:Attribute
    {

    }
}
