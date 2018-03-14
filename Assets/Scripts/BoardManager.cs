using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

	// Serializable makes it serialze for easier storage.  This is an object to store ranges, more or less.
	[Serializable]
	public class Count {
		public int minimum;
		public int maximum;

		public Count(int min, int max) {
			minimum = min;
			maximum = max;
		}
	}

	public int columns = 8;  // How wide the game board is.
	public int rows = 8;  // How tall it is.
	public Count wallCount = new Count(5, 9);  // Establish range for possible number of walls
	public Count foodCount = new Count(1, 5);  // Same with Food.
	public GameObject exit;  // Only 1 exit per map
	public GameObject[] floorTiles;  // These are arrays of GameObjects to hold the tile variations.
	public GameObject[] wallTiles;
	public GameObject[] foodTiles;
	public GameObject[] enemyTiles;
	public GameObject[] outerWallTiles;

	private Transform boardHolder;  // This transform is meant to serve as a parent to keep the Hierarchy view in Unity clean.
	private List<Vector3> gridPositions = new List<Vector3>();  // Stores the list of available grid positions in the board

	// Builds the list of avaiable positions and creates a vector for each x,y position pair
	void InitializeList() {
		// Clear the list if it isn't empty.
		gridPositions.Clear();

		// Working through each X and Y coordinate, assign a vector storing said position into the gridPositions List.
		for (int x = 1; x < columns - 1; x++) {
			for (int y = 1; y < rows - 1; y++) {
				gridPositions.Add(new Vector3(x, y, 0f));  // Note: This vectors final argument is 0f (0 float) because 2D
			}
		}
	}
		
	// This function starts to prep the board, namely putting the outerwalls where they belong and the inner walls on the inside.
	void BoardSetup() {
		// Instantiate the boardHolder for the Hierarchy.
		boardHolder = new GameObject("Board").transform;

		for (int x = -1; x < columns + 1; x++) {
			for (int y = -1; y < rows + 1; y++) {
				// Determine randomly which floor tile sprite to use
				GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

				// If the current position is an outer wall, use an outer wall sprite instead.
				if (x == -1 || x == columns || y == -1 || y == rows)
					toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];

				// Actually create the tile at the approriate location.  Quarternion.identity keeps the tile un-rotated.
				GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

				// Specify the new tile is a child to the boardHolder.
				instance.transform.SetParent(boardHolder);
			}
		}
	}

	// Provide a Vector3 position that is currently unoccupied to generate something randomly into.
	Vector3 RandomPosition() {
		// Get a random index for the list retrieval from the available positions.
		int randomIndex = Random.Range(0, gridPositions.Count);
		// Get the Vector3 for that grid reference
		Vector3 randomPosition = gridPositions[randomIndex];
		// Remove the reference so it doesn't accidentally duplicate
		gridPositions.RemoveAt(randomIndex);

		// Return the actual Vector3 position randomly chosen so it can be used to instantiate.
		return randomPosition;
	}

	// Given an array of objects, place a random amount across the game board.
	void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) {
		// Randomly decide how many to place
		int objectCount = Random.Range(minimum, maximum + 1);
		// Look the approriate number of times to spawn that many of the given tile type.
		for (int i = 0; i < objectCount; i++) {
			// Get a random Vector3 to position the spawn.
			Vector3 randomPosition = RandomPosition();
			// Randomly decide what kind of sprite to use (assuming a choice is available)
			GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
			// Create the selected sprite/object at the randomly determined position.  Un-rotated (Quarternion.identity)
			Instantiate (tileChoice, randomPosition, Quaternion.identity);
		}
	}

	// Called by the GameManager to actually create the board.
	public void SetupScene(int level) {
		BoardSetup();
		InitializeList();
		LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
		LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
		int enemyCount = (int)Mathf.Log(level, 2f); // Logarithmically determine the number of enemies to spawn based on the current game level.
		LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
		Instantiate(exit, new Vector3(columns - 1, rows - 1, 0F), Quaternion.identity);
	}
}
