using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace RedisStudents
{
    class Program
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["RedisConnection"].ToString();

        private static ConnectionMultiplexer Connection
        {
            get
            {
                return ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["RedisConnection"].ToString());
            }
        }

        static void Main(string[] args)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) +
                    "\\Data\\Students.json";

            var content = File.ReadAllLines(filePath);
            var students = JsonConvert.DeserializeObject<Student[]>
                    (string.Join("", content));

            //StudentStringSet(2, students);
            //StudentStringGet();

            var redisCollection = BuildStudentsRedisCollection("vtuStudents", students);
            GetAllStudentsFromRedisList(redisCollection);

            Console.ReadLine();
        }

        private static void GetAllStudentsFromRedisList(RedisList<Student> students)
        {
            for(int i=0; i< students.Count; i++)
            {
                Console.WriteLine(string.Format("{0} {1} {2}", 
                      students[i].ROLL,
                      students[i].USN,
                      students[i].FullName));
            }
        }

        private static void StudentStringSet(int dbValue, IEnumerable<Student> students)
        {
            using (var conn = Connection)
            {
                var db = conn.GetDatabase(dbValue);
                foreach (var student in students)
                {
                    db.StringSet(student.ROLL,
                        JsonConvert.SerializeObject(student));
                }
            }
        }

        private static RedisList<Student> BuildStudentsRedisCollection(string keyName,
            IEnumerable<Student> students)
        {
            var redisCollection = new RedisList<Student>(keyName,
                  Connection);
            foreach(var student in students)
            {
                redisCollection.Add(student);
            }
            return redisCollection;
        }

        private static void StudentStringGet()
        {
            using (var conn = Connection)
            {
                var db = conn.GetDatabase(2);
                var keys = Connection.GetServer(ConnectionString).Keys(2);
                foreach(var key in keys)
                {
                    Console.WriteLine(db.StringGet(key).ToString());
                }
            }
        }
    }
}
