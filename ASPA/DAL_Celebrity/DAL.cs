namespace DAL_Celebrity
{
    public interface IMix<T1, T2>
    {
        List<T2> GetLifeEventsByCelebrityId(int celebrityId); //получить  все события по ID знаменитости
        T1? GetCelebrityByLifeEventId(int lifeEventId);      //получить знаменитость по ID события 
    }

    public interface ICelebrity<T1> : IDisposable
    {
        List<T1> GetAllCelebrities();      //получить все знаменитости 
        T1? GetCelebrityById(int id);       //получить знаменитости по id
        bool DeleteCelebrity(int id);
        bool AddCelebrity(T1 celebrity);
        bool UpdateCelebrity(int id, T1 celebrity);          //изменить по id
        int GetCelebrityByName(string name);                //получить первый id по вхождению подстроки
    }

    public interface ILifeEvent<T2>
    {
        List<T2> GetAllLifeEvents();                //получить все события
        T2? GetLifeEventById(int lifeEventId);          //получить события по id
        bool DeleteLifeEvent(int lifeEventId);          //удалить событие по id
        bool AddLifeEvent(T2 lifeEvent);                //добавить  событие 
        bool UpdateLifeEvent(int id, T2 lifeEvent);      //изменить событие  по id

    }
    // Универсальный интерфейс, объединяющий IMix, ICelebrity и ILifeEvent
    public interface IRepository<T1, T2> : IMix<T1, T2>, ICelebrity<T1>, ILifeEvent<T2>
    {

    }

}