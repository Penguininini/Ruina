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

    static class MyKeywords //the name isn't important
    {
        public static KeywordBuf MultiFold_Def; // this name isn't important either (won't be visible outside of your own dll)
    }

    class BattleUnitBuf_MultiFold_Def : GLHF_UnitBufBase, IRefKeywordBuf // your class name also isn't important
    {
        public ref KeywordBuf KeywordBuf => ref MyKeywords.MultiFold_Def;
        public string KeywordBufName => "MultiFold_Def"; // in most cases you'll want this name to be UNIQUE, so prefix it with your mod id or something; also it must be a valid name for an enum value (so no spaces, punctuation, starting from digits and the like)
        public override KeywordBuf bufType => this.KeywordBuf;

        protected override string keywordIconId => "MultiFold_Def_Icon";

        protected override string keywordId => "MultiFold_Def_Id";

        //the rest of your buf code

        public override void BeforeRollDice(BattleDiceBehavior behavior) 
        {
            base.BeforeRollDice(behavior);

            if (behavior.Detail == BehaviourDetail.Evasion)
            {
                behavior.ApplyDiceStatBonus(new DiceStatBonus { power = stack }); //When rolling evade die, gain power equal to stack
            }
        }

        public override void OnStartBattle()
        {
            base.OnStartBattle();

            DiceCardXmlInfo cardItem = ItemXmlDataList.instance.GetCardItem(new LorId("JohnPrescripts.author",2));

            List<BattleDiceBehavior> list = new List<BattleDiceBehavior>();

            int num = 0;

            foreach (DiceBehaviour diceBehaviour in cardItem.DiceBehaviourList)
            {
                BattleDiceBehavior battleDiceBehavior = new BattleDiceBehavior();
                battleDiceBehavior.behaviourInCard = diceBehaviour.Copy();
                battleDiceBehavior.SetIndex(num++);
                list.Add(battleDiceBehavior);
            }

            _owner.cardSlotDetail.keepCard.AddBehaviours(cardItem, list);
        }
    }

    

    public class PassiveAbility_MultiFold : GLHF_PassiveAbilityBase //Start with 200 multifold def and 21 off, also when using pages spend 1 off for every light, 0 cost will spend 1 off
    {
        public override void OnWaveStart()
        {
            base.OnWaveStart();
            owner.bufListDetail.AddKeywordBufByEtc(MyKeywords.MultiFold_Def, 200, owner);
        }
    }
}
