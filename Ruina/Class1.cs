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
using System.Security.Cryptography.X509Certificates;


//TRY TO KEEP EVERYTHING BUNDLED TOGETHER. FOR EXAMPLE BUFS WOULD BE TOGETHER THEN DICECARDABILITIES ETC.
//ALSO TRY TO KEEP EVERYTHING RELATED WITH AN EFFECT BUNDLED WITHIN THE EFFECT CODE ITSELF. I DO NOT WANT A SPRAWLING MESS OF CODE.
//FOR SUFFICIENTLY LONG PARTS OF CODE (SUCH AS MultiFold_Def) MARK THE END CURLY BRACKET WITH A COMMENT LIKE THIS: END OF MultiFold_Def CODE

namespace Ruina
{


    public class ModInit : ModInitializer
    {
        public static string packageId = "JohnPrescripts.author";
    }

    static class MyKeywords
    {
        public static KeywordBuf MultiFold_Def;
        public static KeywordBuf OverLoad;
        public static KeywordBuf MultiFold_Off;
    }

    class BattleUnitBuf_MultiFold_Def : GLHF_UnitBufBase, IRefKeywordBuf
    {
        public ref KeywordBuf KeywordBuf => ref MyKeywords.MultiFold_Def;
        public string KeywordBufName => "MultiFold_Def";  
        public override KeywordBuf bufType => this.KeywordBuf;

        protected override string keywordIconId => "MultiFold_Def_Icon";

        protected override string keywordId => "MultiFold_Def_Id";


        public void AddCounterDie(int amount) //Simple custom method for adding the 1-1 counter die page. 
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

        public override void OnRoundEnd()
        {
            base.OnRoundEnd();

            if (stack <= 0)
            {
                _owner.bufListDetail.AddKeywordBufThisRoundByEtc(MyKeywords.OverLoad, 1, _owner); //When round ends, gain 1 overload

                BattleUnitBuf overload = _owner.bufListDetail.GetActivatedBuf(MyKeywords.OverLoad);

                int stackGain = Math.Max(0, 200 - (overload.stack * 50)); //Value for Reducing MultiFold_Def regeneration by 50 for each OverLoad stack, with a minimum gain of 0

                if (stackGain > 0)
                {
                    stack += stackGain;
                }
                else
                {
                    _owner.bufListDetail.RemoveBuf(this); //Remove buf when stack gain is 0. I think destroy() works for this as well so try changing it.
                }
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

        public override void OnLoseParrying(BattleDiceBehavior behavior) 
        {
            base.OnLoseParrying(behavior);

            if (behavior.Detail == BehaviourDetail.Evasion && IsAttackDice(behavior.card.target.currentDiceAction.currentBehavior.Detail)) //If any offensive die win against an evade that john has then lose 10 MultiFold_Def
            {
                stack = Math.Max(0, stack - 10); //Prevent value from going below 0
            }
            
        }

        int dmgReduc = 0; //Set damage reduction to 0 for future damage calcs

        public override void BeforeLoseParrying(BattleDiceBehavior behavior)
        {
            base.BeforeLoseParrying(behavior);

            if (IsAttackDice(behavior.Detail) && IsAttackDice(behavior.card.target.currentDiceAction.currentBehavior.Detail)) //Take half damage when being hit by an offensive through an offensive die, take none if clash difference was 2 or lower
            {
                if (behavior.card.target.currentDiceAction.currentBehavior.DiceResultValue - behavior.DiceResultValue <= 2)
                {
                    dmgReduc = 100;
                }
                else
                {
                    dmgReduc = 50;
                }

                stack = Math.Max(0, stack - 2);

            }
        }

        public override int GetDamageReductionRate()
        {
            return dmgReduc;
        }

        public override void AfterTakeDamage(BattleUnitModel attacker, int dmg) //Reset damage reduction to 0 after being hit
        {
            dmgReduc = 0;
        }


        public override void OnStartBattle()
        {
            base.OnStartBattle();
            AddCounterDie(1); //At combat start, add a 1-1 counter die to queue
        }
    } //END OF MultiFold_Def CODE

    class BattleUnitBuf_OverLoad : GLHF_UnitBufBase, IRefKeywordBuf
    {
        public ref KeywordBuf KeywordBuf => ref MyKeywords.OverLoad;
        public string KeywordBufName => "OverLoad";
        public override KeywordBuf bufType => this.KeywordBuf;
        protected override string keywordIconId => "OverLoad_Icon";
        protected override string keywordId => "OverLoad_Id";

    }

    class BattleUnitBuf_MultiFold_Off : GLHF_UnitBufBase, IRefKeywordBuf
    {
        public ref KeywordBuf KeywordBuf => ref MyKeywords.MultiFold_Off;
        public string KeywordBufName => "MultiFold_Off";
        public override KeywordBuf bufType => this.KeywordBuf;
        protected override string keywordIconId => "MultiFold_Off_Icon";
        protected override string keywordId => "MultiFold_Off_Id";


        public override void BeforeRollDice(BattleDiceBehavior behavior)
        {
            base.BeforeRollDice(behavior);

            if (IsAttackDice(behavior.Detail))
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus { power = stack / 10 }); //Gain +1 offensive power for every 10 stacks.
            }
        }

        bool gainedOff = false; //Variable to store if a page has gained MultiFold_Off or not
        public override void OnStartParrying(BattlePlayingCardDataInUnitModel card)
        {
            base.OnStartParrying(card);
            gainedOff = false; //Set to false at the start of a page being used
        }

        public override void OnWinParrying(BattleDiceBehavior behavior)
        {
            base.OnWinParrying(behavior);

            if (IsAttackDice(behavior.Detail) && !gainedOff) //Only proceed if this page has not gained MultiFold_Off
            {
                stack += 2;
                gainedOff = true;
            }
        }

        public override void OnLoseParrying(BattleDiceBehavior behavior)
        {
            base.OnLoseParrying(behavior);

            if (IsAttackDice(behavior.Detail))
            {
                stack = Math.Max(0, stack -2); //Lose 2 stack everyime an offensive die loses a clash
            }
        }

        public override void OnEndParrying()
        {
            base.OnEndParrying();
            gainedOff = false; //Make value false just as a fail safe in case OnStartParrying does not set it to false.
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
            owner.bufListDetail.AddKeywordBufByEtc(MyKeywords.MultiFold_Off, 21, owner);
        }
    }
}
