
using Sandbox;
using Editor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Boomer
{
	[Library( "boomer_path_rail" )]
	[Display( Name = "Path Rail", GroupName = "Boomer", Description = "A rail path the player can slide along." )]
	[Path( "path_generic_node" )]
	[HammerEntity]
	internal partial class RailPathEntity : GenericPathEntity
	{

		[Net, HideInEditor, Change(nameof(OnclPathJsonChanged))]
		public string clPathJson { set; get; }

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
			clPathJson = pathNodesJSON;
		}

		public void OnclPathJsonChanged()
		{
			PathNodes = JsonSerializer.Deserialize<List<BasePathNode>>( clPathJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true } );
		}

		public Vector3 NearestPoint( Vector3 from, bool reverse, out int node, out float t )
		{
			// todo: distance between nodes isn't always the same
			// so basing this on T kinda sucks

			node = 0;
			t = 0f;

			var result = Vector3.Zero;
			var bestDist = float.MaxValue;

			if ( !reverse )
			{
				for ( int i = 0; i < PathNodes.Count - 1; i++ )
				{
					var nodea = PathNodes[i];
					var nodeb = PathNodes[i + 1];

					for ( float j = 0; j <= 1; j += .1f )
					{
						var point = GetPointBetweenNodes( nodea, nodeb, j, false );
						var dist = from.Distance( point );
						if ( dist < bestDist )
						{
							bestDist = dist;
							result = point;
							t = j;
							node = i;
						}
					}
				}
			}
			else
			{
				for ( int i = PathNodes.Count - 1; i >= 1; i-- )
				{
					var nodea = PathNodes[i];
					var nodeb = PathNodes[i - 1];

					for ( float j = 0; j <= 1; j += .1f )
					{
						var point = GetPointBetweenNodes( nodea, nodeb, j, true );
						var dist = from.Distance( point );
						if ( dist < bestDist )
						{
							bestDist = dist;
							result = point;
							t = j;
							node = i;
						}
					}
				}
			}

			return result;
		}

	}
}
