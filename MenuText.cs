using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    public class MenuText : Menu
    {
        public List<string> strings_ = new List<string>();
        public Dictionary<string, Element> childs_text_ = new Dictionary<string, Element>();


        public MenuText()
        {            
        }
        public MenuText(List<string> items)
        {
            setStrings(items);
        }

        public void setStrings(List<string> strings)
        {
            strings_ = strings;

            clearChilds();
            int len = 0;
            int i = 0;
            foreach (var str in strings)
            {
                if (str.Length > len) { len = str.Length; }
                var b = new Button();
                b.setText(str);
                addChild(b, 0, i * 25);
                i++;
            }
            w_ = 10 * len;
            h_ = 25 * strings.Count;

            childs_text_.Clear();
            for (i = 0; i < strings_.Count; i++)
            {
                childs_text_[strings_[i]] = childs_[i];
            }
        }

        public string getStringFromResult(int i)
        {
            if (i >= 0 && i < strings_.Count)
            {
                return strings_[i];
            }
            return "";
        }

        public int getResultFromString(string str)
        {
            for (int i = 0; i < strings_.Count; i++)
            {
                if (str == strings_[i])
                {
                    return i;
                }
            }
            return -1;
        }


























    }
}