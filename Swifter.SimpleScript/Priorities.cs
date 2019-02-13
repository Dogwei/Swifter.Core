namespace Swifter.SimpleScript
{
    public enum Priorities : int
    {
        Bracket = 100,

        Constant = 500,

        GetField = 600,

        SetField = 800,


        InvokeMethod = 1000,

        UnaryOperator = 3000,

        BinaryOperatorHigh = 4000,
        BinaryOperatorMedium = 5000,
        BinaryOperatorLow = 6000,

        CompareOperator = 7000,

        EqualsOperator = 8000,

        ByBitAndOperator = 9010,
        ByBitNonOperator = 9020,
        ByBitOrOperator = 9030,

        AndOperator = 9110,
        OrOperator = 9120,

        ThreeMeshOperator = 9200,

        AssignValueOperator = 10000,

        DefindVar = 20000,

        ParameterSeparator = 500000,

        None = 999999999
    }
}