using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

class ActData{

    private static string[][]? littleRankings; //littles rankings {little name, 1, 2, 3, 4, 5, 6}
    private static string[][]? bigRankings; //bigs rankings {big name, 1, 2, 3, 4, 5, 6}
    private static Dictionary<string, string[]> pairingsBigWise = new Dictionary<string, string[]>(); //big -> littles
    private static Dictionary<string, string> pairingsLittleWise = new Dictionary<string, string>(); //littles -> big
    private static HashSet<string> bigs = new HashSet<string>(); //keeps track of all bigs names
    private static HashSet<string> littles = new HashSet<string>(); //keeps track of all little names

    private static Dictionary<string, string[]> interests = new Dictionary<string, string[]>(); //name -> interest
    private static Dictionary<string, int> bigFreq = new Dictionary<string, int>();

    private static Dictionary<string, int> littleFreq = new Dictionary<string, int>();

    public static void littleData(){
        //takes in csv file of littles rankings
        string path = "littleRankings.csv";
        string[] lines = File.ReadAllLines(path);
        littleRankings = new string[lines.Length][];
        
        //loads csv data into array
        for (int i = 0; i < lines.Length; i++){
            littleRankings[i] = lines[i].Split(",");
        }

        //reformats names to get rid of spaces and makes case insensative
        for (int i = 0; i < littleRankings.Length; i++){
            for (int j = 0; j < littleRankings[i].Length; j++){
                littleRankings[i][j] = littleRankings[i][j].Trim().ToLower();
            }
        }
    }

    public static void bigData(){
        //takes in csv file of bigs rankings
        string path = "bigRankings.csv";
        string[] lines = File.ReadAllLines(path);
        bigRankings = new string[lines.Length][];

        //loads csv data into array
        for (int i = 0; i < lines.Length; i++){
            bigRankings[i] = lines[i].Split(",");
        }

        //reformats names to get rid of spaces and makes case insensative
        for (int i = 0; i < bigRankings.Length; i++){
            for (int j = 0; j < bigRankings[i].Length; j++){
                bigRankings[i][j] = bigRankings[i][j].Trim().ToLower();
            }
        }
    }

    public static void pairingData(){
        //takes in csv file for big-little pairings
        string path = "bigLittlePairings.csv";
        string[] lines = File.ReadAllLines(path);

        //loads csv data into array
        for (int i = 0; i < lines.Length; i++){
            string[] hold = lines[i].Split(",");

            string bigName = hold[0].Trim().ToLower();

            //adds bigs name into set for bigs O(1) search :)
            bigs.Add(bigName);

            //creates an array of littles
            List<string> littlesList = new List<string>();

            //adds littles name into set
            for (int j = 1; j < hold.Length; j++){
                if (!hold[j].Equals("")){
                    string littleName = hold[j].Trim().ToLower();
                    littles.Add(littleName);
                    littlesList.Add(littleName);
                    pairingsLittleWise.Add(littleName, bigName);
                    littleFreq.Add(littleName, 0);
                }
            }
            pairingsBigWise.Add(bigName, littlesList.ToArray());
            bigFreq.Add(bigName, 0);
        }
    }

    public static void LoadInterestsFromCSV(){
        string path = "final_modified_bigLittleInterests.csv";
        string[] lines = File.ReadAllLines(path);

        // Skip the header row by starting from index 1
        for (int i = 1; i < lines.Length; i++){
            string[] split = lines[i].Split(',');

            if (split.Length < 2) // Skip if there are not enough data in the line
                continue;

            string name = split[0].Trim().ToLower();
            string[] keywords = split[1..]  // Use range to get all elements starting from index 1
                            .Where(s => !string.IsNullOrEmpty(s) && s.Trim().ToLower() != "n/a")  // Filter out empty and "N/A" values
                            .Select(s => s.Trim().ToLower())
                            .ToArray();

            if (!string.IsNullOrEmpty(name) && keywords.Length > 0){
                interests[name] = keywords;
            }
        }
    }
    public static void toString(string[][] arr){
        for (int i = 0; i < arr.Length; i++){
            //adds names into string
            string ans = "";
            for (int j = 0; j < arr[i].Length; j++){
                if (!arr[i][j].Equals("")) ans += arr[i][j] + ", ";
            }
            //gets rid of ", " at the end for print formatting
            if (ans.EndsWith(", ")){
                //prints respective pairing/ranking
                Console.WriteLine(ans.Substring(0, ans.Length-2));
            }
        }
    }

    public static void check(string[][] arr, HashSet<string> set1, HashSet<string> set2, string str1, string str2, string str3){
        Console.WriteLine(str3);
        Console.WriteLine("---------------------------------");
        //alpha is to find which column the error is in
        string alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < arr.Length; i++){
            //checks to see if big/little is in set
            if (!set1.Contains(arr[i][0])&& !arr[i][0].Equals("") && !arr[i][0].Equals(" ") && !arr[i][0].Equals("n/a")){
                //prints name discrepency with row and col in csv file
                Console.WriteLine(str1 + arr[i][0] + " - " + i + "-A");
            }
            //checks to see if big/little is in set
            for (int j = 1; j < arr[i].Length; j++){
                if (!set2.Contains(arr[i][j]) && !arr[i][j].Equals("") && !arr[i][j].Equals(" ") && !arr[i][j].Equals("n/a")){
                    //prints name discrepency with row and col in csv file
                    Console.WriteLine(str2 + arr[i][j] + " - " + i + "-" + alpha.Substring(j, 1));
                }
            }
        }
    }

    public static string[] arrayPortion(string[] arr, int start, int end){
        //reportions array using start and end number (works like substring so end is non-inclusive)
        //creates array with correct length
        string[] ans = new string[end-start];
        for (int i = start; i < end; i++){
            //loads name into array
            ans[i-start] = arr[i];
        }
        //returns array
        return ans;
    }

    public static void bigsTopThree(){
        //keeps track of the amount of bigs that ranked a little that they received in their top 3
        int count = 0;
        //loops through a bigs ranking row
        foreach (var bigRanking in bigRankings){
            //instantiates big
            string big = bigRanking[0];
            //creates a set of the bigs' top 3 (set of O(1) :) )
            var set = new HashSet<string>(arrayPortion(bigRanking, 1, 4));
            //accesses the ace pairing using hashmap
            if (pairingsBigWise.TryGetValue(big, out var pairedLittles)){
                //checks if little is in ace pairing
                foreach (var little in pairedLittles){
                    if (set.Contains(little)){
                        count++;
                        break;
                    }
                }
            }
        }
        //divide the amount of bigs that got AT LEAST 1 LITTLE in their top 3 by amount of pairings
        double percentage = Math.Round(((double)count/bigs.Count)*100, 2);
        Console.WriteLine("Bigs That Got Someone in Top 3: " + percentage + "%");
    }
    public static void bigsTopSix(){
        //keeps track of the amount of bigs that ranked a little that they received in their top 3
        int count = 0;
        //loops through a bigs ranking row
        foreach (var bigRanking in bigRankings){
            //instantiates big
            string big = bigRanking[0];
            //creates a set of the bigs' top 3 (set of O(1) :) )
            var set = new HashSet<string>(arrayPortion(bigRanking, 1, 7));
            //accesses the ace pairing using hashmap
            if (pairingsBigWise.TryGetValue(big, out var pairedLittles)){
                //checks if little is in ace pairing
                foreach (var little in pairedLittles){
                    if (set.Contains(little)){
                        count++;
                        break;
                    }
                }
            }
        }
        //divide the amount of bigs that got AT LEAST 1 LITTLE in their top 3 by amount of pairings
        double percentage = Math.Round(((double)count/bigs.Count)*100, 2);
        Console.WriteLine("Bigs That Got Someone in Top 6: " + percentage + "%");
    }
    public static void littlesTopThree(){
        //keeps track of the amount of bigs that ranked a little that they received in their top 3
        int count = 0;
        //loops through a bigs ranking row
        foreach (var littleRanking in littleRankings){
            //instantiates big
            string  little = littleRanking[0];
            //creates a set of the bigs' top 3 (set of O(1) :) )
            var set = new HashSet<string>(arrayPortion(littleRanking, 1, 4));
            //accesses the ace pairing using hashmap
            if (pairingsLittleWise.TryGetValue(little, out var pairedBig)){
                //checks if little is in ace pairing
                if (set.Contains(pairedBig)) count++;
            }
        }
        //divide the amount of bigs that got AT LEAST 1 LITTLE in their top 3 by amount of pairings
        double percentage = Math.Round(((double)count/littles.Count)*100, 2);
        Console.WriteLine("Littles That Got Someone in Top 3: " + percentage + "%");
    }

    public static void littlesTopSix(){
        //keeps track of the amount of bigs that ranked a little that they received in their top 3
        int count = 0;
        //loops through a bigs ranking row
        foreach (var littleRanking in littleRankings){
            //instantiates big
            string  little = littleRanking[0];
            //creates a set of the bigs' top 3 (set of O(1) :) )
            var set = new HashSet<string>(arrayPortion(littleRanking, 1, 7));
            //accesses the ace pairing using hashmap
            if (pairingsLittleWise.TryGetValue(little, out var pairedBig)){
                //checks if little is in ace pairing
                if (set.Contains(pairedBig)) count++;
            }
        }
        //divide the amount of bigs that got AT LEAST 1 LITTLE in their top 3 by amount of pairings
        double percentage = Math.Round(((double)count/littles.Count)*100, 2);
        Console.WriteLine("Littles That Got Someone in Top 6: " + percentage + "%");
    }
    public static void mutualChoices(){
        int count = 0;
        foreach (var bigRanking in bigRankings){
            string big = bigRanking[0];
            //Get top 6 choices of big
            var topSixLittles = new HashSet<string>(arrayPortion(bigRanking, 1, 7));

            foreach (var littleRanking in littleRankings){
                string little = littleRanking[0];
                // Get top 6 choices of little
                var topSixBigs = new HashSet<string>(arrayPortion(littleRanking, 1, 7));

                if (topSixLittles.Contains(little) && topSixBigs.Contains(big)){
                    count++;
                }
            }
        }
        double percentage = Math.Round(((double)count/(bigs.Count+littles.Count))*100, 2);
        Console.WriteLine("Bigs/Little Pairings That Matched: " + percentage + "%");
    }

    public static void popularBigs(){
        for (int i = 0; i < littleRankings.Length; i++){
            for (int j = 1; j < littleRankings[i].Length; j++){
                if (!littleRankings[i][j].Equals("n/a") && !littleRankings[i][j].Equals("")) bigFreq[littleRankings[i][j]]++;
            }
        }
        var top5 = bigFreq.OrderByDescending(x => x.Value).Take(5).Select(x => x.Key).ToList();
        foreach (var key in top5){
            Console.WriteLine(key + " - " + bigFreq[key]);
        }
    }
    public static void popularLittles(){
        for (int i = 0; i < bigRankings.Length; i++){
            for (int j = 1; j < bigRankings[i].Length; j++){
                if (!bigRankings[i][j].Equals("n/a")) littleFreq[bigRankings[i][j]]++;
            }
        }
        var top5 = littleFreq.OrderByDescending(x => x.Value).Take(5).Select(x => x.Key).ToList();
        foreach (var key in top5){
            Console.WriteLine(key + " - " + littleFreq[key]);
        }
    }

    public static void unmatchedInterests(){
        Dictionary<string, List<string>> unmatchedBigs = new Dictionary<string, List<string>>();
        int totalUnmatchedPairs = 0;
        int pairsWithCommonInterests = 0;

        foreach (var bigRanking in bigRankings){
            string big = bigRanking[0];
            var topSixLittles = new HashSet<string>(arrayPortion(bigRanking, 1, 7));

            if (pairingsBigWise.TryGetValue(big, out var pairedLittles)){
                foreach (var little in pairedLittles){
                    if (littleRankings.Any(lr => lr[0] == little)){
                        var littleRanking = littleRankings.First(lr => lr[0] == little);
                        var topSixBigs = new HashSet<string>(arrayPortion(littleRanking, 1, 7));

                        if (!topSixLittles.Contains(little) || !topSixBigs.Contains(big)){
                            totalUnmatchedPairs++;

                            if (!unmatchedBigs.ContainsKey(big)){
                                unmatchedBigs[big] = new List<string>();
                            }

                            unmatchedBigs[big].Add(little);

                            if (interests.TryGetValue(big, out var bigInterests) && interests.TryGetValue(little, out var littleInterests)){
                                var commonInterests = bigInterests.Intersect(littleInterests).ToList();
                                if (commonInterests.Any()){
                                    pairsWithCommonInterests++;
                                }
                            }
                        }
                    }
                }
            }
        }

        double percentage = totalUnmatchedPairs > 0 ? (double)pairsWithCommonInterests / totalUnmatchedPairs * 100 : 0;
        percentage = Math.Round(percentage, 2);
        Console.WriteLine("Unmatched Pairings That had Similar Interests: " + percentage + "%");
    }

    static void Main(){
        littleData();
        bigData();
        pairingData();
        LoadInterestsFromCSV();
        Console.WriteLine("----- BOTH DATA -----");
        mutualChoices();
        unmatchedInterests();
        Console.WriteLine("----- BIGS DATA -----");
        bigsTopThree();
        bigsTopSix();
        Console.WriteLine("----- LITTLES DATA -----");
        littlesTopThree();
        littlesTopSix();
        Console.WriteLine("----- POPULAR BIGS -----");
        popularBigs();
        Console.WriteLine("----- POPULAR LITTLES -----");
        popularLittles();
    }
}