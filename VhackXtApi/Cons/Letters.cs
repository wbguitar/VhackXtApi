using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VhackXtApi.Console
{
    //TODO: move to a suitable package -> eg. OCR
public class Letters {

    private static Dictionary<Int64, char> table;

    private static Letters instance;

    private Letters() {}

    public static Letters Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Letters();
                initializeTable();
            }
            return instance;
        }
    }

    private static void initializeTable() {
        table = new Dictionary<Int64, char>();
        table[Convert.ToInt64("138882308926605356273569103872")] =  'b';
        table[Convert.ToInt64("79228163665150427101350723584")] =  'c';
        table[Convert.ToInt64("80160259584121600695424712704")] =  'd';
        table[Convert.ToInt64("79228163628474025430208086016")] =  'e';
        table[Convert.ToInt64("88574609755369126824638414848")] =  'f';
        table[Convert.ToInt64("79228164834502758305282048894")] =  'g';
        table[Convert.ToInt64("138882308926605356272980262912")] =  'h';
        table[Convert.ToInt64("86684818004401202618987053056")] =  'i';
        table[Convert.ToInt64("81092326386798553849915688572")] =  'j';
        table[Convert.ToInt64("109055235572362849156480565248")] =  'k';
        table[Convert.ToInt64("96588451065184023330955198464")] =  'l';
        table[Convert.ToInt64("79228165887414256860221079552")] =  'm';
        table[Convert.ToInt64("79228166589176383304397684736")] =  'n';
        table[Convert.ToInt64("79228163628473959472445521920")] =  'o';
        table[Convert.ToInt64("79228166589176383304986575040")] =  'p';
        table[Convert.ToInt64("79228163610099272992790610691")] =  'q';
        table[Convert.ToInt64("79228166617755272836240375808")] =  'r';
        table[Convert.ToInt64("79228164852659503468501663744")] =  's';
        table[Convert.ToInt64("79286422279248852407983931392")] =  't';
        table[Convert.ToInt64("79228166125485765668778999808")] =  'u';
        table[Convert.ToInt64("79228166125492547562131292160")] =  'w';
        table[Convert.ToInt64("79228166118746221705078964224")] =  'x';
        table[Convert.ToInt64("79228166125485765668779033470")] =  'y';
        table[Convert.ToInt64("79228164838989840740951916544")] =  'z';

        table[Convert.ToInt64("86728823591756243744717996032")] =  'A';
        table[Convert.ToInt64("157458676833660938652659482624")] =  'B';
        table[Convert.ToInt64("98536831751724949324173279232")] =  'C';
        table[Convert.ToInt64("157458676774212579146438803456")] =  'D';
        table[Convert.ToInt64("158070379020604335802487275520")] =  'E';
        table[Convert.ToInt64("158379864030425680871207993344")] =  'F';
        table[Convert.ToInt64("98536827029360440086489858048")] =  'G';
        table[Convert.ToInt64("139814404441260521983823773696")] =  'H';
        table[Convert.ToInt64("88519994809135780404085129216")] =  'J';
        table[Convert.ToInt64("118252401752678178141694197760")] =  'I';
        table[Convert.ToInt64("139818074106331162837887680512")] =  'K';
        table[Convert.ToInt64("138882308407357485899275173888")] =  'L';
        table[Convert.ToInt64("139858209552890160595930906624")] =  'M';
        table[Convert.ToInt64("139853317623935778623905267712")] =  'N';
        table[Convert.ToInt64("86728823591756177372312240128")] =  'O';
        table[Convert.ToInt64("158074020020646975708147482624")] =  'P';
        table[Convert.ToInt64("97921498009831347988704591872")] =  'Q';
        table[Convert.ToInt64("158074020020662751526803144704")] =  'R';
        table[Convert.ToInt64("118459924531798346654457659392")] =  'S';
        table[Convert.ToInt64("158175968019631692007184269312")] =  'T';
        table[Convert.ToInt64("139814404436937066339978969088")] =  'U';
        table[Convert.ToInt64("79228166125459485660819226624")] =  'V';
        table[Convert.ToInt64("139814404438673230644480507904")] =  'W';
        table[Convert.ToInt64("139813962754173539958442688512")] =  'Y';
        table[Convert.ToInt64("139813962754173579878748848128")] =  'X';
        table[Convert.ToInt64("157844637121106498363852718080")] =  'Z';

        table[Convert.ToInt64("97921498009831321548885852160")] =  '0';
        table[Convert.ToInt64("86724069724311004751297642496")] =  '1';
        table[Convert.ToInt64("97921494454385875554809085952")] =  '2';
        table[Convert.ToInt64("117843685324194040540873621504")] =  '3';
        table[Convert.ToInt64("81102140209191565922604023808")] =  '4';
        table[Convert.ToInt64("158070379535474496230586580992")] =  '5';
        table[Convert.ToInt64("97921493233935897933245054976")] =  '6';
        table[Convert.ToInt64("158150481074817586777085181952")] =  '7';
        table[Convert.ToInt64("97921496284530170325943189504")] =  '8';
        table[Convert.ToInt64("97921498003163530644575485952")] =  '9';

        table[Convert.ToInt64("83590080654423668100862705664")] =  '{';
        table[Convert.ToInt64("113919611614311278351370682368")] =  '}';
        table[Convert.ToInt64("98474489125036455632123592704")] =  '[';
        table[Convert.ToInt64("117618867732573307059834716160")] =  ']';
        table[Convert.ToInt64("79228162514336113712605167616")] =  '-';
        table[Convert.ToInt64("79228162514264337593544015616")] =  '_';


    }

    public char getCharFor(Int64 hash) {
        if (table.ContainsKey(hash))
            return table[hash];

        return ' ';
    }

}

}
