
namespace Komodo.Runtime
{
    public struct Interaction
    {
        public int sourceEntity_id;
        public int targetEntity_id;
        public int interactionType;

        public Interaction(int sourceEntity_id, int targetEntity_id, int interactionType)
        {
            this.sourceEntity_id = sourceEntity_id;
            this.targetEntity_id = targetEntity_id;
            this.interactionType = interactionType;
        }
    }
}