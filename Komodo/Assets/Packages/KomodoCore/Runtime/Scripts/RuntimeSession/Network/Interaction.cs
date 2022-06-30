
namespace Komodo.Runtime
{
    /// <summary>
    /// A struct for all interactions that happen in Komodo. This struct stores information about who starts the interaction, the target being interacted with, and the type of interaction occurs.
    /// </summary>
    public struct Interaction
    {
        public int sourceEntity_id;
        public int targetEntity_id;
        public int interactionType;

        /// <summary>
        /// Initialize interactions.
        /// </summary>
        /// <param name="sourceEntity_id">user that starts an interaction.</param>
        /// <param name="targetEntity_id">the target that is being interacted by the user.</param>
        /// <param name="interactionType">the type of interaction.</param>
        public Interaction(int sourceEntity_id, int targetEntity_id, int interactionType)
        {
            this.sourceEntity_id = sourceEntity_id;
            this.targetEntity_id = targetEntity_id;
            this.interactionType = interactionType;
        }
    }
}