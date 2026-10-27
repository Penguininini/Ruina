using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLHF;
using AutoKeywordUtil;
using ConvertBufAggregate;
using EnumExtenderV2;
using FrameworkLoader;
using LOR_DiceSystem;


namespace Ruina
{


    public class ModInit : ModInitializer
    {
        public static string packageId = "JohnPrescripts.author";
    }

    static class MyKeywords
    {
        public static KeywordBuf MultiFold_Def;
    }

    class BattleUnitBuf_MultiFold_Def : GLHF_UnitBufBase, IRefKeywordBuf //
    {
        public ref KeywordBuf KeywordBuf => ref MyKeywords.MultiFold_Def;
        public string KeywordBufName => "MultiFold_Def";  
        public override KeywordBuf bufType => this.KeywordBuf;

        protected override string keywordIconId => "MultiFold_Def_Icon";

        protected override string keywordId => "MultiFold_Def_Id";

        

        public void AddCounterDie(int amount) //Simple method for adding the 1-1 counter die page. 
        {
            DiceCardXmlInfo card = ItemXmlDataList.instance.GetCardItem(new LorId("JohnPrescripts.author", 2)); //Page id 2 is the 1-1 counter die page

            for (int i = 0; i < amount; i++)
            {
                List<BattleDiceBehavior> dice = new List<BattleDiceBehavior>();

                int num = 0;

                foreach (DiceBehaviour diceBehaviour in card.DiceBehaviourList)
                {
                    BattleDiceBehavior behavior = new BattleDiceBehavior();
                    behavior.behaviourInCard = diceBehaviour.Copy();
                    behavior.SetIndex(num++);
                    behavior.AddAbility(new DiceCardAbility_CounterEvadeWin()); //Give the counter die its ability to add more counter dice when winning a clash.
                    dice.Add(behavior);
                }

                _owner.cardSlotDetail.keepCard.AddBehaviours(card, dice);
            }
        }


        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            if (behavior.Detail == BehaviourDetail.Evasion)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus { power = stack }); //When rolling evade die, gain power equal to stack

                if (IsDefenseDice(behavior.card.target.currentDiceAction.currentBehavior.Detail)) //If a defensive die clashes with any evade on john, lose 5 MultiFold_Def
                {
                    stack = Math.Max(0, stack - 5);
                }

                if (IsAttackDice(behavior.card.target.currentDiceAction.currentBehavior.Detail)) //If an offensive die clashes with any evade on john, lose 1 MultiFold_Def
                {
                    stack = Math.Max(0, stack - 1);
                }
            }
        }

        public override void OnLoseParrying(BattleDiceBehavior behavior) //If any offensive die win against an evade that john has then lose 10 MultiFold_Def
        {
            base.OnLoseParrying(behavior);

            if (behavior.Detail == BehaviourDetail.Evasion && IsAttackDice(behavior.card.target.currentDiceAction.currentBehavior.Detail))
            {
                stack = Math.Max(0, stack - 10); //Prevent value from going below 0
            }
            
        }

        public override void OnStartBattle()
        {
            base.OnStartBattle();
            AddCounterDie(1); //At combat start, add a 1-1 counter die to queue
        }
    }

    public class DiceCardAbility_CounterEvadeWin : DiceCardAbilityBase //When the counter die wins a clash, add 2 more counter dice to the queue
    {
        public override void OnWinParrying()
        {
            base.OnWinParrying();

            BattleUnitBuf_MultiFold_Def buf = owner.bufListDetail.GetActivatedBuf(MyKeywords.MultiFold_Def) as BattleUnitBuf_MultiFold_Def;
            if (buf != null)
            {
                buf.AddCounterDie(2);
            }
        }
    }
    

    public class PassiveAbility_MultiFold : GLHF_PassiveAbilityBase //TODO: Start with 200 multifold def and 21 off, also when using pages spend 1 off for every light, 0 cost will spend 1 off
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddKeywordBufByEtc(MyKeywords.MultiFold_Def, 200, owner);
        }
    }
}
