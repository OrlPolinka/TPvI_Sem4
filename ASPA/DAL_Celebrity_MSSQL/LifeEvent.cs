using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Celebrity_MSSQL
{
    public class LifeEvent          //событие жизни знаменитости 
    {
        public LifeEvent() { }

        public int Id { get; set; }         //id события 
        public int CelebrityId { get; set; }        //id знаменитости 
        public DateTime Date { get; set; }              //дата события

        public string Description { get; set; }         //описаниние события 

        public string? ReqPhotoPath { get; set; }        //reguest path  фотографии 

        //public virtual bool Update(LifeEvent lifeEvent); //вспомогтаелный метод 
    }
}
