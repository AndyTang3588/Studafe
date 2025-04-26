
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Variable Holder")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VariableHolder : UdonSharpBehaviour
    {
        public Cup_Coffee scriptCup;
        public Tool scriptTool;

        /// <summary>
        /// Cup                                     <para></para>
        /// 0_State : 0_None, 1_Machine, 2_Plate    <para></para>
        /// 1_Index : -1_None 0~_index of deposit   <para></para>
        /// 
        /// Filter                                  <para></para>
        /// 0_State : 1_Grinder, 2_Machine          <para></para>
        /// 1_Last Index of Machine                 <para></para>
        /// 
        /// Pitcher                                 <para></para>
        /// 0_State : 1_Machine                     <para></para>
        /// 1_Last Index of Machine                 <para></para>
        /// </summary>
        [UdonSynced] public sbyte[] m_deposit;

        
        /// <summary>
        /// Cup                                                         <para></para>
        /// 0_Content - 0_None, 1-5                                     <para></para>
        /// 1_Type, 2_Top, 3_Cream                                      <para></para>
        /// Glass                                                       <para></para>
        /// 4_FruitSlice - 1_Lemon, 2_Grapefruit, 3_Grape, 4_Strawberry <para></para>
        /// 5_Garnish - 1_Mint, 2-3_Rosemary, 4-5_Thyme                 <para></para>
        /// 6_Straw - 0_None, 1~6_Straw                                 <para></para>
        /// 7_Ice = 0_None, 1_Ice                                       <para></para>
        ///                                                             <para>.</para>
        /// Filter                                                      <para></para>
        /// 0_Content - 0_Empty, 1_Grinded, 2_Tampered, 3_Extracted     <para></para>
        /// 1_NozzleCount - 1 or 2                                      <para></para>
        /// 2_Index of content material                                 <para></para>
        ///                                                             <para>.</para>
        /// Pitcher                                                     <para></para>
        /// 0_Content - 0_Empty, 1_Milk, 2_Latte, 3_Cappuccino          <para></para>
        /// </summary>
        [UdonSynced] public byte[] m_info;

        // Cup Type List
        // 0_Empty
        // 1_Chocolate, 2_Caramel
        // 11_Water, 12_Espresso, 13_Espresso w/ Chocolate, 14_Espresso w/ Caramel, 17_Milk, 18_Milk w/ Caramel
        // 21_Americano, 31-34_Latte, 35-36_Cappuccino, 37_Caramel Macchiato
        // 41~44_Mocha
        // 61_JucieLemon, 62_AdeLemon, 63_JuiceGrape 64_AdeGrape
        // 65_JuiceGraprfruit, 66_AdeGrapefruit, 67_JuiceStrawberry, 68_AdeStrawberry
        
        // Cup Top List
        // 0_None
        // Powders : 1_Cocoa, 2_Cinamon
        // Syrups : 3_Chocolate, 4_Caramel
        
        // Cream
        // 0_None, 1_Stain, 2~3_Cream

        public override void OnDeserialization()
        {
            if (scriptCup) scriptCup.LocalUpdate();
            else if (scriptTool) scriptTool.LocalUpdate();
        }
    }
}