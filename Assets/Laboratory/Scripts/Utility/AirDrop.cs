using UnityEngine;

public class AirDrop
{
	public static Vector3 RandomPosition(float yOffset)
	{
		var spawnPosition = new Vector3(
			Random.Range(-30.0f, 30.0f),
			yOffset,
			Random.Range(-30.0f, 30.0f));
		return spawnPosition;
	}

	public static Quaternion RandomRotation()
	{
		var spawnRotation = Quaternion.Euler(
			1.0f,
			Random.Range(0, 180),
			0.0f);

		return spawnRotation;
	}
}
