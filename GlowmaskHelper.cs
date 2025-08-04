using Terraria.ModLoader;

namespace GlowmaskHelper
{
	public class GlowmaskHelper : Mod
	{
		public bool IsLoading { get; internal set; }

        public override void Load()
        {
            IsLoading = true;
        }
    }
}
