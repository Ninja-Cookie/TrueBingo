using Reptile;
using System;

namespace TrueBingo
{
    public static class BingoConfigTypes
    {
        public enum Outfits
        {
            Spring,
            Summer,
            Autumn,
            Winter
        }

        public enum Styles
        {
            Skateboard,
            Inline,
            BMX
        }

        public enum Characters
        {
            Vinyl,
            Frank,
            Coil,
            Red,
            Tryce,
            Bel,
            Rave,
            DOTEXE,
            Solace,
            DJCyber,
            Eclipse,
            DevilTheory,
            Faux,
            FleshPrince,
            Irene,
            Felix,
            OldHead,
            Base,
            Jay,
            Mesh,
            Futurism,
            Rise,
            Shine,
            Faux_NoJetpack,
            DOTEXE_Boss,
            Felix_Red
        }

        public enum Stages
        {
            Hideout,
            Versum,
            Square,
            Brink,
            Mall,
            Pyramid,
            Mataan,
            Random
        }

        public static Reptile.Characters GetCharacter(Characters character)
        {
            return (Reptile.Characters)((int)character);
        }

        public static MoveStyle GetStyle(Styles style)
        {
            switch(style)
            {
                case Styles.Skateboard: return MoveStyle.SKATEBOARD;
                case Styles.Inline:     return MoveStyle.INLINE;
                case Styles.BMX:        return MoveStyle.BMX;

                default: return MoveStyle.SKATEBOARD;
            }
        }

        public static int GetOutfit(Outfits outfit)
        {
            return (int)outfit;
        }

        public static Reptile.Stage GetStage(Stages stage)
        {
            switch(stage)
            {
                case Stages.Hideout:    return Stage.hideout;
                case Stages.Versum:     return Stage.downhill;
                case Stages.Square:     return Stage.square;
                case Stages.Brink:      return Stage.tower;
                case Stages.Mall:       return Stage.Mall;
                case Stages.Pyramid:    return Stage.pyramid;
                case Stages.Mataan:     return Stage.osaka;

                default: return Stage.hideout;
            }
        }
    }
}
