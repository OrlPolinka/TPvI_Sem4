using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Celebrity_MSSQL
{
    public class Celebrity          //знаменитость 
    {
        public Celebrity() { this.FullName = string.Empty; this.Nationality = string.Empty; }
        public int Id { get; set; }                 //id знамкнитости 
        public string FullName { get; set; }
        public string Nationality { get; set; }

        public string? ReqPhotoPath { get; set; }           //reguest path  фотографии 

        //public  virtual bool Update(Celebrity celebrity);   //вспомогательный метод
    }
}
