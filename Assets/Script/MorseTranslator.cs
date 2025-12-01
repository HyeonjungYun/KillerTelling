using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class MorseTranslator
{
    // ÇÑ±Û ÀÚ¸ð ºÐ¸®¸¦ À§ÇÑ ¹è¿­
    private static readonly string[] ChoSung = { "¤¡", "¤¢", "¤¤", "¤§", "¤¨", "¤©", "¤±", "¤²", "¤³", "¤µ", "¤¶", "¤·", "¤¸", "¤¹", "¤º", "¤»", "¤¼", "¤½", "¤¾" };
    private static readonly string[] JungSung = { "¤¿", "¤À", "¤Á", "¤Â", "¤Ã", "¤Ä", "¤Å", "¤Æ", "¤Ç", "¤È", "¤É", "¤Ê", "¤Ë", "¤Ì", "¤Í", "¤Î", "¤Ï", "¤Ð", "¤Ñ", "¤Ò", "¤Ó" };
    private static readonly string[] JongSung = { "", "¤¡", "¤¢", "¤£", "¤¤", "¤¥", "¤¦", "¤§", "¤©", "¤ª", "¤«", "¤¬", "¤­", "¤®", "¤¯", "¤°", "¤±", "¤²", "¤´", "¤µ", "¤¶", "¤·", "¤¸", "¤º", "¤»", "¤¼", "¤½", "¤¾" };

    // ¸ð½º ºÎÈ£ ¸ÅÇÎ (ÇÑ±Û/¿µ¹®/¼ýÀÚ)
    private static readonly Dictionary<string, string> MorseMap = new Dictionary<string, string>()
    {
        // ÇÑ±Û ÀÚÀ½
        {"¤¡", ".-.."}, {"¤¤", "..-."}, {"¤§", "-..."}, {"¤©", "...-"}, {"¤±", "--"}, {"¤²", ".--"}, {"¤µ", "--."}, {"¤·", "-.-"}, {"¤¸", ".--."}, {"¤º", "-.-."}, {"¤»", "-..-"}, {"¤¼", "--.."}, {"¤½", "---"}, {"¤¾", ".---"},
        // ÇÑ±Û ¸ðÀ½
        {"¤¿", "."}, {"¤Á", ".."}, {"¤Ã", "-"}, {"¤Å", "..."}, {"¤Ç", ".-"}, {"¤Ë", "-."}, {"¤Ì", "...."}, {"¤Ð", ".-."}, {"¤Ñ", "-.."}, {"¤Ó", "..-"}, {"¤À", "--.-"}, {"¤Ä", "-.--"},
        // ¿µ¹®
        {"A", ".-"}, {"B", "-..."}, {"C", "-.-."}, {"D", "-.."}, {"E", "."}, {"F", "..-."}, {"G", "--."}, {"H", "...."}, {"I", ".."}, {"J", ".---"}, {"K", "-.-"}, {"L", ".-.."}, {"M", "--"}, {"N", "-."}, {"O", "---"}, {"P", ".--."}, {"Q", "--.-"}, {"R", ".-."}, {"S", "..."}, {"T", "-"}, {"U", "..-"}, {"V", "...-"}, {"W", ".--"}, {"X", "-..-"}, {"Y", "-.--"}, {"Z", "--.."},
        // Æ¯¼ö
        {" ", " / "}
    };

    public static string TextToMorse(string input)
    {
        StringBuilder sb = new StringBuilder();

        foreach (char c in input)
        {
            if (c >= '°¡' && c <= 'ÆR') // ÇÑ±ÛÀÎ °æ¿ì ÀÚ¼Ò ºÐ¸®
            {
                int unicodeIndex = c - 0xAC00;
                int cho = unicodeIndex / (21 * 28);
                int jung = (unicodeIndex % (21 * 28)) / 28;
                int jong = unicodeIndex % 28;

                sb.Append(GetMorse(ChoSung[cho]));
                sb.Append(GetMorse(JungSung[jung]));
                if (jong != 0) sb.Append(GetMorse(JongSung[jong]));

                sb.Append(" "); // ±ÛÀÚ °£ °ø¹é
            }
            else // ¿µ¹®, ¼ýÀÚ, Æ¯¼ö¹®ÀÚ
            {
                sb.Append(GetMorse(c.ToString().ToUpper()));
                sb.Append(" ");
            }
        }
        return sb.ToString();
    }

    private static string GetMorse(string key)
    {
        return MorseMap.ContainsKey(key) ? MorseMap[key] : "?";
    }

    public static string[] TextToMorseArray(string input)
    {
        List<string> morseList = new List<string>();

        foreach (char c in input)
        {
            StringBuilder sb = new StringBuilder();

            if (c >= '°¡' && c <= 'ÆR')
            {
                int unicodeIndex = c - 0xAC00;
                int cho = unicodeIndex / (21 * 28);
                int jung = (unicodeIndex % (21 * 28)) / 28;
                int jong = unicodeIndex % 28;

                sb.Append(GetMorse(ChoSung[cho]));
                sb.Append(GetMorse(JungSung[jung]));
                if (jong != 0) sb.Append(GetMorse(JongSung[jong]));
            }
            else
            {
                sb.Append(GetMorse(c.ToString().ToUpper()));
            }

            // ±ÛÀÚ °£ ±¸ºÐÀ» À§ÇØ µÚ¿¡ °ø¹é ÇÏ³ª Ãß°¡ (Áß¿ä)
            sb.Append(" ");

            morseList.Add(sb.ToString());
        }

        return morseList.ToArray();
    }
}
