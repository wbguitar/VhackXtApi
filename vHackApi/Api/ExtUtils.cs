using System;

namespace vHackApi.Api
{
    public static class ExtUtils
    {
        /// <summary>
        /// Parse all the properties of an object and copies them to related properties of the destination object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool CopyProperties<T>(this T from, ref T to)
        {
            try
            {
                foreach (var prop in from.GetType().GetProperties())
                {
                    var val = prop.GetValue(from);
                    prop.SetValue(to, val);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}