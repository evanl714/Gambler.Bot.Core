using System;
using System.Collections.Generic;
using System.Text;

namespace Gambler.Bot.Core.Helpers
{

    public abstract class PersistentBase
    {
        public int Id { get; set; }
    }
    public class PersistentTableName : Attribute
    {
        public string TableName { get; set; }
        public PersistentTableName(string TableName)
        {
            this.TableName = TableName;
        }
    }
    public class NonPersistent : Attribute
    {

    }
}
