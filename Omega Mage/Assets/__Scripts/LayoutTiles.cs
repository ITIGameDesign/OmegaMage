using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class TileTex {
	// This class enables us to define various textures for tiles
	public string str;
	public Texture2D tex;
}

[System.Serializable]
public class EnemyDef {
	// This class enables us to define various enemies
	public string str;
	public GameObject go;
}

public class LayoutTiles : MonoBehaviour {
	static public LayoutTiles S;
	public TextAsset roomsText; // The Rooms.xml file
	public string roomNumber = "0"; // Current room # as a string
	// ^ roomNumber as string allows encoding in the XML & rooms 0-F
	public GameObject tilePrefab; // Prefab for all Tiles
	public TileTex[] tileTextures; // A list of named textures for Tiles
	public GameObject portalPrefab; // Prefab for the portals between rooms
	public EnemyDef[] enemyDefinitions; // Prefabs for Enemies

	public bool ________________;

	private bool firstRoom = true; // Is this the first room built?
	public PT_XMLReader roomsXMLR;
	public PT_XMLHashList roomsXML;
	public Tile[,] tiles;
	public Transform tileAnchor;
	void Awake() {
		S = this; // Set the Singleton for LayoutTiles
		// Make a new GameObject to be the TileAnchor (the parent transform of
		// all Tiles). This keeps Tiles tidy in the Hierarchy pane.
		GameObject tAnc = new GameObject("TileAnchor");
		tileAnchor = tAnc.transform;
		// Read the XML
		roomsXMLR = new PT_XMLReader(); // Create a PT_XMLReader
		roomsXMLR.Parse(roomsText.text); // Parse the Rooms.xml file
		roomsXML = roomsXMLR.xml["xml"][0]["room"]; // Pull all the <room>s
		// Build the 0th Room
		BuildRoom(roomNumber);
	}
	// This is the GetTileTex() method that Tile uses
	public Texture2D GetTileTex(string tStr) {
		// Search through all the tileTextures for the proper string
		foreach (TileTex tTex in tileTextures) {
			if (tTex.str == tStr) {
				return(tTex.tex);
			}
		}
		// Return null if nothing was found
		return(null);
	}

	// Build a room based on room number. This is an alternative version of
	// BuildRoom that grabs roomXML based on <room> num.
	public void BuildRoom(string rNumStr) {
		PT_XMLHashtable roomHT = null;
		for (int i=0; i<roomsXML.Count; i++) {
			PT_XMLHashtable ht = roomsXML[i];
			if (ht.att("num") == rNumStr) {
				roomHT = ht;
				break;
			}
		}
		if (roomHT == null) {
			Utils.tr("ERROR","LayoutTiles.BuildRoom()",
			         "Room not found: "+rNumStr);
			return;
		}
		BuildRoom(roomHT);
	}

	// Build a room from an XML <room> entry
	public void BuildRoom(PT_XMLHashtable room) {

		// Destroy any old Tiles
		foreach (Transform t in tileAnchor) { // Clear out old tiles
			// ^ You can iterate over a Transform to get its children
			Destroy(t.gameObject);
		}
		// Move the Mage out of the way
		Mage.S.pos = Vector3.left * 1000;
		// ^ This keeps the Mage from accidentally triggering OnTriggerExit() on
		// a Portal. In my testing, I found that OnTriggerExit was being called
		// at strange times.
		Mage.S.ClearInput(); // Cancel any active mouse input and drags
		string rNumStr = room.att("num");

		// Get the texture names for the floors and walls from <room> attributes
		string floorTexStr = room.att("floor");
		string wallTexStr = room.att("wall");
		// Split the room into rows of tiles based on carriage returns in the
		// Rooms.xml file
		string[] roomRows = room.text.Split('\n');
		// Trim tabs from the beginnings of lines. However, we're leaving spaces
		// and underscores to allow for non-rectangular rooms.
		for (int i=0; i<roomRows.Length; i++) {
			roomRows[i] = roomRows[i].Trim('\t');
		}
		// Clear the tiles Array
		tiles = new Tile[ 100, 100 ]; // Arbitrary max room size is 100x100
		// Declare a number of local fields that we'll use later
		Tile ti;
		string type, rawType, tileTexStr;
		GameObject go;
		int height;
		float maxY = roomRows.Length-1;
		List<Portal> portals = new List<Portal>();

		// These loops scan through each tile of each row of the room
		for (int y=0; y<roomRows.Length; y++) {
			for (int x=0; x<roomRows[y].Length; x++) {
				// Set defaults
				height = 0;
				tileTexStr = floorTexStr;
				// Get the character representing the tile
				type = rawType = roomRows[y][x].ToString();
				switch (rawType) {
				case " ": // empty space
				case "_": // empty space
					// Just skip over empty space
					continue;
				case ".": // default floor
					// Keep type="."
					break;
				case "|": // default wall
					height = 1;
					break;
				default:
					// Anything else will be interpreted as floor
					type = ".";
					break;
				}
				// Set the texture for floor or wall based on <room> attributes
				if (type == ".") {
					tileTexStr = floorTexStr;
				} else if (type == "|") {
					tileTexStr = wallTexStr;
				}
				// Instantiate a new TilePrefab
				go = Instantiate(tilePrefab) as GameObject;
				ti = go.GetComponent<Tile>();
				// Set the parent Transform to tileAnchor
				ti.transform.parent = tileAnchor;
				// Set the position of the tile
				ti.pos = new Vector3( x, maxY-y, 0 );
				tiles[x,y] = ti; // Add ti to the tiles 2D Array
				// Set the type, height, and texture of the Tile
				ti.type = type;
				ti.height = height;
				ti.tex = tileTexStr;
				// More to come here...

				// If the type is still rawType, continue to the next iteration
				if (rawType == type) continue;

				// Check for specific entities in the room
				switch (rawType) { // 1
				case "X": // Starting position for the Mage
					// Mage.S.pos = ti.pos; // Uses the Mage Singleton
					if (firstRoom) {
						Mage.S.pos = ti.pos; // Uses the Mage Singleton
						roomNumber = rNumStr;
						// ^ Setting roomNumber now keeps any portals from
						// moving the Mage to them in this first room.
						firstRoom = false;
					}


					break;

				case "0": // Numbers are room portals (up to F in hexadecimal)
				case "1": // This allows portals to be placed in the Rooms.xml file
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
				case "A":
				case "B":
				case "C":
				case "D":
				case "E":
				case "F":
					// Instantiate a Portal
					GameObject pGO = Instantiate(portalPrefab) as GameObject;
					Portal p = pGO.GetComponent<Portal>();
					p.pos = ti.pos;
					p.transform.parent = tileAnchor;
					// ^ Attaching this to the tileAnchor means that the Portal
					// will be Destroyed when a new room is built
					p.toRoom = rawType;
					portals.Add(p);
					break;
				
				default:
					// Try to see if there's an Enemy for that letter
					Enemy en = EnemyFactory(rawType);
					if (en == null) break; // If there's not one, break out
					// Set up the new Enemy
					en.pos = ti.pos;
					// Make en a child of tileAnchor so it's deleted when the
					// next room is loaded.
					en.transform.parent = tileAnchor;
					en.typeString = rawType;
					break;
				}

				/// will be more
			}
		}

		// Position the Mage
		foreach (Portal p in portals) {
			// If p.toRoom is the same as the room number the Mage just exited,
			// then the Mage should enter this room through this Portal
			// Alternatively, if firstRoom == true and there was no X in the
			// room (as a default Mage starting point), move the Mage to this
			// Portal as a backup measure (if, for instance, you want to just
			// load room number "5")
			if (p.toRoom == roomNumber || firstRoom) {
				// ^ If there's an X in the room, firstRoom will be set to false
				// by the time the code gets here
				Mage.S.StopWalking(); // Stop any Mage movement
				Mage.S.pos = p.pos; // Move _Mage to this Portal location
				// _Mage maintains her facing from the previous room, so there
				// is no need to rotate her in order for her to enter this room
				// facing the right direction.
				p.justArrived = true;
				// ^ Tell the Portal that Mage has just arrived.
				firstRoom = false;
				// ^ Stops a 2nd Portal in this room from moving the Mage to it
			}
		}
		// Finally assign the roomNumber
		roomNumber = rNumStr;
	}
///////pg 718 below

	public Enemy EnemyFactory(string sType) {
		// See if there's an EnemyDef with that sType
		GameObject prefab = null;
		foreach (EnemyDef ed in enemyDefinitions) {
			if (ed.str == sType) {
				prefab = ed.go;
				break;
			}
		}
		if (prefab == null) {
			Utils.tr("LayoutTiles.EnemyFactory()","No EnemyDef for: "+sType);
			return(null);
		}
		GameObject go = Instantiate(prefab) as GameObject;
		// The generic form of GetComponent (with the <>) won't work for
		// interfaces like Enemy, so we must use this form instead.
		Enemy en = (Enemy) go.GetComponent(typeof(Enemy));
		return(en);
	}
}