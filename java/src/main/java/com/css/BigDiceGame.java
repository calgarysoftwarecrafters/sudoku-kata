package com.css;

public class BigDiceGame {
    public static int CalculateScore(ScoringType st, int die1, int die2, int d3, int dieFour, int fifthDie) {
        final int bigScore = 50;
        switch (st) {
            case Pair: {
                final int value = 2;
                int[] tallies1;
                tallies1 = new int[6];
                tallies1[die1 - 1]++;
                tallies1[die2 - 1]++;
                tallies1[d3 - 1]++;
                tallies1[dieFour - 1]++;
                tallies1[fifthDie - 1]++;
                for (int i = 6 - 1; i >= 0; i--) {
                    if (tallies1[i] >= value) {
                        return (i + 1) * value;
                    }
                }

                return 0;
            }

            case TwoPair: {
                int[] counts1 = new int[6];
                counts1[die1 - 1]++;
                counts1[die2 - 1]++;
                counts1[d3 - 1]++;
                counts1[dieFour - 1]++;
                counts1[fifthDie - 1]++;
                int n = 0;
                int score = 0;
                for (int i = 0; i < 6; i += 1) {
                    if (counts1[6 - i - 1] >= 2) {
                        n++;
                        score += 6 - i;
                    }
                }

                if (n == 2) {
                    return score * 2;
                }

                return 0;
            }

            case FullHouse: {
                int[] tallies;
                boolean _2 = false;
                int i;
                int _2_at = 0;
                boolean _3 = false;
                int _3_at = 0;


                tallies = new int[6];
                tallies[die1 - 1] += 1;
                tallies[die2 - 1] += 1;
                tallies[d3 - 1] += 1;
                tallies[dieFour - 1] += 1;
                tallies[fifthDie - 1] += 1;

                for (i = 0; i != 6; i += 1) {
                    if (tallies[i] == 2) {
                        _2 = true;
                        _2_at = i + 1;
                    }
                }

                for (i = 0; i != 6; i += 1) {
                    if (tallies[i] == 3) {
                        _3 = true;
                        _3_at = i + 1;
                    }
                }

                if (_2 && _3) {
                    return _2_at * 2 + _3_at * 3;
                }

                return 0;
            }

            case FiveOfAKind: {
                int[] dice1 = {die1, die2, d3, dieFour, fifthDie};
                int[] counts2 = new int[6];
                for (int die : dice1) {
                    counts2[die - 1]++;
                }

                for (int i1 = 0; i1 != 6; i1++) {
                    if (counts2[i1] == 5) {
                        return bigScore;
                    }
                }

                return 0;
            }
        }

        return 0;
    }
}
