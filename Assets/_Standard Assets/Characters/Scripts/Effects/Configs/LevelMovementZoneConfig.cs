using StandardAssets.Characters.Attributes;
using UnityEngine;

namespace StandardAssets.Characters.Effects.Configs
{
	/// <summary>
	/// Configuration of default effects for different zones in a level
	/// </summary>
	[CreateAssetMenu(fileName = "Level Movement Zone Configuration",
		menuName = "Standard Assets/Characters/Level Movement Zone Configuration", order = 1)]
	public class LevelMovementZoneConfig : ScriptableObject
	{
		[MovementZoneId(), SerializeField, Tooltip("The default movement event zone ID")]
		protected string defaultZoneId;
		
		[SerializeField, Tooltip("List of movement event libraries for different movement zones")]
		protected MovementEventZoneDefinitionList zonesDefinition;

		/// <summary>
		/// Gets the Gets the <see cref="MovementEventLibrary"/> for a specified zoneId for a specified zoneId
		/// </summary>
		/// <param name="zoneId">The zoneId needed to look up the <see cref="MovementEventLibrary"/></param>
		/// <value>Gets the <see cref="MovementEventLibrary"/> for a specified zoneId. returns null if the zoneId does not have an associated <see cref="MovementEventLibrary"/></value>
		public MovementEventLibrary this[string zoneId]
		{
			get
			{
				return zonesDefinition[zoneId];
			}
		}

		/// <summary>
		/// Gets the default <see cref="MovementEventLibrary"/>
		/// </summary>
		public MovementEventLibrary defaultLibrary
		{
			get { return this[defaultZoneId]; }
		}
		
		/// <summary>
		/// Gets the default Ids
		/// </summary>
		public string defaultId
		{
			get { return defaultZoneId; }
		}
	}
}