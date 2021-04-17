package com.css;

import org.approvaltests.combinations.CombinationApprovals;
import org.junit.jupiter.api.Test;
import org.lambda.functions.Function6;

import static org.junit.jupiter.api.Assertions.assertEquals;

class PinningTest {

    @Test
    void pinEverything() {
        Integer [] dice = new Integer [] {1, 2, 3, 4, 5, 6};
        CombinationApprovals.verifyAllCombinations(BigDiceGame::CalculateScore, ScoringType.values(), dice, dice, dice, dice, dice);
    }
}
