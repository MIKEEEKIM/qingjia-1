using qingjia_MVC.Models;
using System.Collections.Generic;
using System.Linq;

namespace qingjia_MVC.Common
{
    public class PublicMethod
    {
        public class ClassList
        {
            private string _id;
            private string _name;

            public string ID
            {
                get { return _id; }
                set { _id = value; }
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
            
            public ClassList(string id,string name)
            {
                _id = id;
                _name = name;
            }
        }

        //获取所有班级
        public static List<ClassList> getClassList()
        {
            Entities db = new Entities();
            var classList = from t in db.T_Class
                            select new
                            {
                                ID = t.ID,
                                Name = t.ClassName
                            };

            List<ClassList> classes = new List<ClassList>();
            foreach (var t in classList)
            {
                classes.Add(new ClassList(t.ID,t.Name));
            }

            return classes;
        }

        //根据辅导员ID获取班级列表
        public static List<ClassList> getClassList(string teacherID)
        {
            Entities db = new Entities();
            var classList = from t in db.T_Class
                            where t.TeacherID == teacherID
                            orderby t.ID
                            select new
                            {
                                ID = t.ID,
                                Name = t.ClassName
                            };

            List<ClassList> classes = new List<ClassList>();
            foreach (var t in classList)
            {
                classes.Add(new ClassList(t.ID, t.Name));
            }

            return classes;
        }



    }
}