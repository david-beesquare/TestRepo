using System;
using System.Collections.Generic;

namespace MiniJSONWrapper
{
    /// <summary>
    /// Interface for deserializing data from a json into the object that implements this interface
    /// </summary>
    public interface IJsonDeserializer
    {
        void OnJsonDeserialize(JsonObject jsonObject);
    }

    /// <summary>
    /// Interface for serializing data from the object that is implementing this interface to a given json
    /// </summary>
    public interface IJsonSerializer
    {
        void OnJsonSerialize(JsonObject jsonObject);
    }

    public class JsonArray
    {
        public JsonArray()
        {
            m_list = new List<object>();
        }

        public JsonArray(List<object> list)
        {
            m_list = list;
        }

        public void Add(object objectValue)
        {
            m_list.Add(objectValue);
        }

        public void Remove(object objectValue)
        {
            m_list.Remove(objectValue);
        }

        public List<object> GetList()
        {
            return m_list;
        }

        public List<T> BuildList<T>() where T : struct
        {
            List<T> list = new List<T>(m_list.Count);
            foreach (object obj in m_list)
            {
                // FIXME: This hack is due to the fact that at the moment MiniJSON's ParseNumber() method
                // either returns a long (Int64) or a double value. If we wanted a list of ints or floats
                // the "is" test would fail (and not having it would yield casting exceptions)
                // Also, we can't OR the test because casting to T will throw an exception. We could handle it
                // and then attempt conversions, but I'd rather split explicitly until we reach a final
                // solution directly on MiniJSON
                if (obj is T)
                {
                    list.Add((T)obj);
                }
                else if ((typeof(T).Equals(typeof(int)) && obj is long))
                {
                    list.Add((T)(object)Convert.ToInt32(obj));
                }
                else if ((typeof(T).Equals(typeof(float)) && obj is double))
                {
                    list.Add((T)(object)Convert.ToSingle(obj));
                }
            }
            return list;
        }

        public List<string> BuildStringList()
        {
            List<string> list = new List<string>(m_list.Count);
            foreach (object obj in m_list)
            {
                if (obj is string)
                {
                    list.Add((string)obj);
                }
            }
            return list;
        }

        public List<T> BuildObjectList<T>() where T : IJsonDeserializer, new()
        {
            List<T> list = new List<T>(m_list.Count);
            foreach (object dictionary in m_list)
            {
                T obj = new T();
                JsonObject jsonObj = new JsonObject(dictionary);

                obj.OnJsonDeserialize(jsonObj);
                list.Add(obj);
            }
            return list;
        }

        public List<JsonObject> BuildJsonObjectList ()
        {
            List<JsonObject> list = new List<JsonObject>(m_list.Count);
            foreach (object dictionary in m_list)
            {                
                JsonObject jsonObj = new JsonObject(dictionary);                
                list.Add(jsonObj);
            }
            return list;
        }

        public override string ToString()
        {
            return MiniJSON.Json.Serialize(m_list);
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public bool IsEmpty()
        {
            return (m_list.Count == 0);
        }

        private List<object> m_list;
    }

    public class JsonObject
    {
        public JsonObject()
        {
            m_dictionary = new Dictionary<string, object>();
        }

        public JsonObject(string stringToParse)
        {
            m_dictionary = MiniJSON.Json.Deserialize(stringToParse) as Dictionary<string, object>;
        }

        public JsonObject(object dict)
        {
            m_dictionary = (Dictionary<string, object>)dict;
        }

        public bool HasKey(string key)
        {
            return m_dictionary.ContainsKey(key);
        }

        public void Clear()
        {
            m_dictionary.Clear();
        }

        public static JsonObject GetSerializedObject(IJsonSerializer jsonSerializer)
        {
            if (jsonSerializer != null)
            {
                JsonObject jsonObject = new JsonObject();
                jsonSerializer.OnJsonSerialize(jsonObject);
                return jsonObject;
            }
            else
            {
                return null;
            }
        }

        public static void FillObject(string jsonString, IJsonDeserializer jsonDeserializerObj)
        {
            if (jsonDeserializerObj != null)
            {
                JsonObject jsonObj = new JsonObject(jsonString);
                if (jsonObj != null)
                {
                    jsonDeserializerObj.OnJsonDeserialize(jsonObj);
                }
            }
        }

        #region Bool

        public bool GetBool(string key, bool defaultValue = false)
        {
            return (HasKey(key) ? (bool)(m_dictionary[key]) : defaultValue);
        }

        public void SetBool(string key, bool boolValue)
        {
            m_dictionary[key] = boolValue;
        }

        #endregion

        #region Int

        public int GetInt(string key, int defaultValue = 0)
        {
            return (HasKey(key) ? (int)((long)m_dictionary[key]) : defaultValue);
        }

        public void SetInt(string key, int intValue)
        {
            m_dictionary[key] = intValue;
        }

        #endregion

        #region Long

        public long GetLong(string key, long defaultValue = 0)
        {
            return (HasKey(key) ? (long)m_dictionary[key] : defaultValue);
        }

        public void SetLong(string key, long longValue)
        {
            m_dictionary[key] = longValue;
        }

        #endregion

        #region Float

        public float GetFloat(string key, float defaultValue = 0.0f)
        {
			return (HasKey(key) ? Convert.ToSingle(m_dictionary[key]) : defaultValue);
        }

        public void SetFloat(string key, float floatValue)
        {
            m_dictionary[key] = floatValue;
        }

        #endregion

        #region String

        public string GetString(string key, string defaultValue = "")
        {
            return (HasKey(key) ? (string)(m_dictionary[key]) : defaultValue);
        }

        public void SetString(string key, string stringValue)
        {
            m_dictionary[key] = stringValue;
        }

        #endregion

        #region Array

        public JsonArray GetArray(string key)
        {
            if (HasKey(key))
            {
                return new JsonArray((List<object>)m_dictionary[key]);
            }
            else
            {
                return null;
            }
        }

        public List<T> GetList<T>(string key) where T : struct
        {
            return GetArray(key).BuildList<T>();
        }

        public List<string> GetStringList(string key)
        {
            return GetArray(key).BuildStringList();
        }

        public List<T> GetObjectList<T>(string key) where T : IJsonDeserializer, new()
        {
            return GetArray(key).BuildObjectList<T>();
        }

        public void SetArray(string key, JsonArray objectValue)
        {
            m_dictionary[key] = objectValue.GetList();
        }

        public void SetList<T>(string key, List<T> list) where T : struct
        {
            List<object> objList = new List<object>(list.Count);
            foreach (T listElement in list)
            {
                objList.Add(listElement);
            }

            m_dictionary[key] = objList;
        }

        public void SetStringList(string key, List<string> list)
        {
            List<object> objList = new List<object>(list.Count);
            foreach (string listElement in list)
            {
                objList.Add(listElement);
            }

            m_dictionary[key] = objList;
        }

        public void SetObjectList<T>(string key, List<T> list) where T : IJsonSerializer
        {
            List<object> objList = new List<object>(list.Count);
            foreach (T objSerializer in list)
            {
                JsonObject jsonObj = GetSerializedObject(objSerializer);
                objList.Add(jsonObj.m_dictionary);
            }

            m_dictionary[key] = objList;
        }

        #endregion

        #region JsonObject

        public JsonObject GetObject(string key)
        {
            return (HasKey(key) ?
                    (m_dictionary[key] != null ? new JsonObject(m_dictionary[key]) : null) :
                    null);
        }

        public void SetObject(string key, JsonObject objectValue)
        {
            m_dictionary[key] = objectValue != null ? objectValue.m_dictionary : null;
        }

        #endregion

        public override string ToString()
        {
            return MiniJSON.Json.Serialize(m_dictionary);
        }

        private Dictionary<string, object> m_dictionary;
    }
}