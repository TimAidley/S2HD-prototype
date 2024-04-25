// These 'using' sections tell the compiler which libraries are being used by the code.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Audio;

// The namespace is a way to group classes together. It's a bit like a folder.
namespace IntelOrca.Sonic
{
	// This is a class that represents a level. A class is a way a representing a type of object in code.
	class Level
	{
		// A field that is a reference to the main game object.
		// 'private' means that this field is only used within this class.
		// 'SonicGame' is the type of the field.
		// 'mGame' is the name of the field.
		private SonicGame mGame;

		// The name of the level. 'string' is the type and is used to hold text.
		private string mName;
		// These are 'indices' that represent the zone and act of the level, like zone 3 of act 1 etc.
		// 'int' is the type and is used to hold whole numbers (integers).
		private int mZoneIndex;
		private int mActIndex;

		// The starting coordinates of the level
		private int mStartX;
		private int mStartY;
		
		// Both of the following fields are rectangles.
		// This is the area of the level that the player is allowed to move in.
		private Microsoft.Xna.Framework.Rectangle mPlayerBoundary;
		// Visible boundary is the area of the level that is visible to the player.
		private Microsoft.Xna.Framework.Rectangle mVisibleBoundary;

		// A list of all the sound effects used in the level.
		private List<Sound> mSounds = new List<Sound>();

		// The level appears to be stored in chunks. Each chunk is a 128x128 block of the level.
		// although 128x128 of what? pixels? blocks? Probably blocks.
		// Level chunks
		// The number of columns and rows of chunks in the level.
		private int mChunkColumns;
		private int mChunkRows;
		// An array of bytes that represents the chunks.
		// The array is two dimensional, so you can think of it as a grid.
		// A byte is a small number that can be between 0 and 255.
		public byte[,] mChunkLayout;
		// private List<Chunk> mChunks = new List<Chunk>();
		
		// The Landscale class also seems to contain an array of chunks, and a forground and background layer.
		// Not clear to me what the difference is between the Landscape and Level classes.
		private Landscape mLandscape;
		
		// Maybe the folder it is loaded from?
		private string mDirectory;

		// A list of object definitions and an object manager
		private List<LevelObjectDefinition> mObjectDefinitions = new List<LevelObjectDefinition>();
		private LevelObjectManager mObjects = new LevelObjectManager();

		// This is the 'constructor' function for the class. It is the first thing called when a 'Level' object is created.
		// You can tell it is the constructor because it has the same name as the class, and has no return value.
		// The (SonicGame game) section shows that you have to pass a 'SonicGame' object to it. All it does is save that in
		// the mGame field.
		public Level(SonicGame game)
		{
			mGame = game;
		}

		// Loads a level from a directory.
		// This is a normal member function. Here are what the sections of this line mean:
		// 'public' means that this function can be called from outside the class.
		// 'void' is the return type. 'void' is a special word that means there is no return value.
		// 'Load' is the name of the function.
		// Everything within the parentheses ( ... ) are the 'parameters' that the function takes.
		// 'string directory' means that the function takes a string as a parameter, and it is called 'directory'.
		public void Load(string directory)
		{
			// Saves the folder in the mDirectory field.
			mDirectory = directory;
			// The name seems to get hard-coded to 'Emerald Hill', and zome and act are hard-coded to 1.
			mName = "Emerald Hill";
			mZoneIndex = 1;
			mActIndex = 1;

			// Player and visible boundaries are set up. I'm not sure why they are set as they are.
			// The rectangle is set up by providing the coordinates of the bottom left corner, followed
			// by the width and height of the rectangle.
			mPlayerBoundary = new Microsoft.Xna.Framework.Rectangle(16, 0, 11264 - 16, 1024);
			mVisibleBoundary = new Microsoft.Xna.Framework.Rectangle(0, 0, 10976, 1024);

			// A new Landscape object is created by passing the path to the landscape file.
			mLandscape = new Landscape(mDirectory + "\\landscape.dat");
			// .. and it is instructed to load itself.
			LoadLayout(mDirectory + "\\act1.dat");

			// The following code has been commented out. Perhaps the chunk loading code was moved to landscape
			// and was not completely removed?
			// for (int i = 0; i < 256; i++)
				// mChunks.Add(new Chunk());

			// LoadChunks(directory + "\\chunks.dat", directory + "\\fgback.png", directory + "\\fgfront.png");
		}

		// This function restarts the level.
		public void Restart()
		{
			// A new object manager is created. The old one if it exists will get automatically thrown away by garbage collection.
			mObjects = new LevelObjectManager();
			// The sounds list is cleared.
			mSounds.Clear();

			// The boundaries are reset.
			mPlayerBoundary = new Microsoft.Xna.Framework.Rectangle(16, 0, 11264 - 16, 1024);
			mVisibleBoundary = new Microsoft.Xna.Framework.Rectangle(0, 0, 10976, 1024);

			// The objects are cleared. Unsure why this needs to be done seeing as the object manager was just recreated.
			mObjects.Clear();
			
			// 'foreach' is a way of going through all the objects in a collection.
			// In this case it loops through all the definitions in mObjectDefinitions.
			// Inside the loop 'def' points to the current definition.
			foreach (LevelObjectDefinition def in mObjectDefinitions) {
				// Create an object using the object definition
				LevelObject obj = LevelObject.Create(mGame, this, def);
				
				// 'null' is a special value that means 'nothing'. If the object was not created, obj will be null.
				// So this if statement checks if the object was created, and if it was, it adds it to the object manager.
				if (obj != null)
					mObjects.Add(obj);
			}
		}

		// This function updates the level, and it is called once per frame.
		public void Update()
		{
			// Clear the sounds
			mSounds.Clear();

			// tells all the objects to update.
			mObjects.Update();
		}

		// This function is used to get the closest character to a particular object in a level.
		// 'public' means that this function can be called from outside the class.
		// 'Character' is the type of object returned by the function.
		// 'GetClosestCharacter' is the name of the function.
		// ( The parameters are between the parentheses. )
		// 'LevelObject obj' is the object that you want to find the closest character to.
		// all of the other parameters are 'out' parameters, which means that any values they have when calling the function are ignored,
		// but they will have their values set when the function is called.
		// This means that directionX and Y, and the two distance values will be set by the function when it exits.
		public Character GetClosestCharacter(LevelObject obj, out int directionX, out int directionY, out int horizontalDistance, out int verticalDistance)
		{
			// This creates a list to hold characters in
			List<Character> characters = new List<Character>();
			
			// This goes through all the objects in the level, and adds them to the list if they are characters.
			foreach (LevelObject o in mObjects)
				if (o is Character)
					characters.Add((Character)o);

			
			// This variable will be used to store the closest character so far
			Character closestCharacter = null;
			// This is used to store the horizontal distance to the closest character so far.
			int lowestHorizontalDistance = -1;

			// This sets the 'out' parameters to 0.
			directionX = 0;
			directionY = 0;
			horizontalDistance = 0;
			verticalDistance = 0;

			
			// The algorithm works like this - it goes through each character, and works out the horizontal and vertical distance to the object.
			// Then for that character, if no closest character is set or the horizontal distance is less than the lowest horizontal distance so far,
			// The closest character is set to that character, and the horizontal and vertical distances are set to the distances to that character,
			// and the closest distance so far is set.
			foreach (Character character in characters) {
				// Work out the horizontal and vertical distances to the object. These can be negative.
				int hDist = obj.DisplacementX - character.DisplacementX;
				int vDist = obj.DisplacementY - character.DisplacementY;
				
				// 'Mathf.Abs' is a function that returns the absolute value of a number.
				// The absolute value of a number is the number you get if you ignore the any minus signs.
				// So Math.Abs(4) is 4, and Math.Abs(-4) is also 4.
				int ahDist = Math.Abs(hDist);

				// 'if' is a conditional statement. If the condition in the parentheses is true, the code directly after the if statement is run.
				// '!=' means 'not equal to'.
				// '>=' means 'greater than or equal to'.
				// '&&' means logical AND.
				// 'continue' means 'skip to the next iteration of the loop'.
				// So this line means 'if the closest character is not null and the absolute horizontal distance is greater than
				// or equal to the lowest horizontal distance so far, then continue to the next character'.
				if (closestCharacter != null && ahDist >= lowestHorizontalDistance)
					continue;

				// If the code has got here then 
				closestCharacter = character;
				horizontalDistance = hDist;
				verticalDistance = vDist;
				
				// As the distance to the object can be positive or negative, a positive distance means the character is to the object's right,
				// and a negative is to the object's left.
				// The same for being above or below and object

				// Is player to object's left
				if (hDist < 0)
					directionX = 2;

				// Is player under object
				if (vDist < 0)
					directionY = 2;
			}

			return closestCharacter;
		}

		// Loads a layout from a file. The path to the file is passed as a parameter.
		// Nothing (void) is returned.
		private void LoadLayout(string path)
		{
			// This sets up a file stream to read the file, and a binary reader to read it with.
			// binary is how you want to read a file when it's not in some kind of text format.
			FileStream fs = new FileStream(path, FileMode.Open);
			BinaryReader br = new BinaryReader(fs);

			// This reads two strings from the file, but doesn't do anything with them.
			// I wonder what's in them?
			br.ReadString();
			br.ReadString();

			// Chunk layout
			// Next it reads the chunk layout. The number of rows and columns of chunks are read in as 'Int32' values.
			// An Int32 is a whole (integer) number held in 32 bits that can represent numbers from -2,147,483,648 to 2,147,483,647.
			mChunkColumns = br.ReadInt32();
			mChunkRows = br.ReadInt32();
			
			// This creates a two-dimensional array of bytes to hold the chunk layout, of the size read in above.
			mChunkLayout = new byte[mChunkColumns, mChunkRows];
			
			// These are two 'for' loops for reading in the chunks.
			// A 'for' loop is a way of repeating a block of code a certain number of times.
			// There are three parts to a 'for' loop, separated by semicolons.
			// The first part is the initialization section, and a variable is normally declared and initialized there.
			// The second part is the condition section, and the loop will continue as long as the condition is true.
			// the third part is the increment section, and it is run after each iteration of the loop.
			// So in this case the initialization section is 'int y = 0' which declares an int called 'y' and sets it to zero.
			// Then for each iteration of the loop, it checks the condition 'y < mChunkRows', and if it is true, it runs the code in the loop.
			// otherwise it exits.
			// After each iteration, it runs the increment section 'y++' which adds one to 'y'.
			// you can see that for each value of y there is also a loop that goes through each column, x.
			// and within that loop, it reads a single byte for each chunk.
			for (int y = 0; y < mChunkRows; y++)
				for (int x = 0; x < mChunkColumns; x++)
					mChunkLayout[x, y] = br.ReadByte();

			// Objects
			// Next it loads in object definitions.
			// Starting off by clearing the definitions list.
			mObjectDefinitions.Clear();
			// Then it reads in the number of objects
			int numObjects = br.ReadInt32();
			// And loops that many times to read in each object.
			// the loop variable 'i' is commonly used in loops - it stands for 'index'.
			for (int i = 0; i < numObjects; i++) {
				// Creates a new definition object
				LevelObjectDefinition definition = new LevelObjectDefinition();

				// Reads in the values for the definition.
				definition.Id = br.ReadInt32();				// The ID of the object - maybe this is unique?
				definition.SubType = br.ReadInt32();		// Subtype probably means that there can be variations on object types?
				definition.DisplacementX = br.ReadInt32();	// X and y start positions of the object
				definition.DisplacementY = br.ReadInt32();
				definition.Respawn = br.ReadBoolean();		// Whether the object should be able to respawn?
				definition.FlipY = br.ReadBoolean();		// Whether the object should be flipped vertically
				definition.FlipX = br.ReadBoolean();		// Whether the object should be flipped horizontally

				// If the id is 1, it means the player, so the player start position is stored from this object position.
				if (definition.Id == 1) {
					mStartX = definition.DisplacementX;
					mStartY = definition.DisplacementY;
				}

				// Creates a level object from the definition, and adds it to the object list.
				LevelObject obj = LevelObject.Create(mGame, this, definition);
				if (obj != null)
					mObjects.Add(obj);

				// Adds the definition to the object definitions list.
				mObjectDefinitions.Add(definition);
			}

			// close the binary reader and file.
			br.Close();
			fs.Close();
		}

		public void FindCeiling(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = y + 1; i < y + 32; i++) {
					if (IsSolid(x, i, layer, lrb, t))
						continue;
					distance = y - i;
					angle = GetAngle(x, i, layer);
					return;
				}

				distance = -32;
			}

			for (int i = y - 1; i > y - 32; i--) {
				if (!IsSolid(x, i, layer, lrb, t))
					continue;
				distance = y - i - 1;
				angle = GetAngle(x, i, layer);
				return;
			}

			distance = 32;
		}

		public void FindFloor(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = y - 1; i > y - 32; i--) {
					if (IsSolid(x, i, layer, lrb, t))
						continue;
					distance = i - y;
					angle = GetAngle(x, i, layer);
					return;
				}

				distance = -32;
			}

			for (int i = y + 1; i < y + 32; i++) {
				if (!IsSolid(x, i, layer, lrb, t))
					continue;
				distance = i - y - 1;
				angle = GetAngle(x, i, layer);
				return;
			}

			distance = 32;
		}

		public void FindWallLeft(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = x + 1; i < x + 32; i++) {
					if (IsSolid(i, y, layer, lrb, t))
						continue;
					distance = x - i;
					angle = GetAngle(i, y, layer);
					return;
				}

				distance = -32;
			}

			for (int i = x - 1; i > x - 32; i--) {
				if (!IsSolid(i, y, layer, lrb, t))
					continue;
				distance = x - i - 1;
				angle = GetAngle(i, y, layer);
				return;
			}

			distance = 32;
		}

		public void FindWallRight(int x, int y, int layer, bool lrb, bool t, out int distance, ref int angle)
		{
			if (IsSolid(x, y, layer, lrb, t)) {
				// Check other way
				for (int i = x - 1; i > x - 32; i--) {
					if (IsSolid(i, y, layer, lrb, t))
						continue;
					distance = i - x;
					angle = GetAngle(i, y, layer);
					return;
				}

				distance = -32;
			}

			for (int i = x + 1; i < x + 32; i++) {
				if (!IsSolid(i, y, layer, lrb, t))
					continue;
				distance = i - x - 1;
				angle = GetAngle(i, y, layer);
				return;
			}

			distance = 32;
		}

		public bool IsSolid(int x, int y, int layer, bool lrb, bool t)
		{
			int chunkX = x / 128;
			int chunkY = y / 128;
			int blockX = (x % 128);
			int blockY = (y % 128);

			if (x < 0 || y < 0)
				return false;

			if (chunkX >= mChunkColumns || chunkY >= mChunkRows)
				return false;

			Landscape.Chunk chunk = mLandscape.Chunks[mChunkLayout[chunkX, chunkY]];
			Landscape.Chunk.Layer clayer = (layer == 0 ? chunk.BackLayer : chunk.FrontLayer);
			
			bool solid = false;
			if (lrb)
				solid = clayer.CollisionLRB[blockX, blockY];
			if (t && !solid)
				solid = clayer.CollisionT[blockX, blockY];
			return solid;
		}

		private int GetAngle(int x, int y, int layer)
		{
			int chunkX = x / 128;
			int chunkY = y / 128;
			int blockX = (x % 128) / 16;
			int blockY = (y % 128) / 16;

			if (x < 0 || y < 0)
				return 0;

			if (chunkX >= mChunkColumns || chunkY >= mChunkRows)
				return 0;

			Landscape.Chunk chunk = mLandscape.Chunks[mChunkLayout[chunkX, chunkY]];
			Landscape.Chunk.Layer clayer = (layer == 0 ? chunk.BackLayer : chunk.FrontLayer);
			return clayer.Angles[blockX, blockY];
		}

		public void AddSound(SoundEffect soundEffect, int x, int y)
		{
			AddSound(new Sound(soundEffect, x, y));
		}

		public void AddSound(Sound sound)
		{
			mSounds.Add(sound);
		}

		public IEnumerable<Sound> Sounds
		{
			get
			{
				return mSounds;
			}
		}

		public int Width
		{
			get
			{
				return mChunkColumns;
			}
		}

		public int Height
		{
			get
			{
				return mChunkRows;
			}
		}

		public byte[,] LevelLayout
		{
			get
			{
				return mChunkLayout;
			}
		}

		public LevelObjectManager Objects
		{
			get
			{
				return mObjects;
			}
		}

		public string Name
		{
			get
			{
				return mName;
			}
		}

		public int ZoneIndex
		{
			get
			{
				return mZoneIndex;
			}
		}

		public int ActIndex
		{
			get
			{
				return mActIndex;
			}
		}

		public Microsoft.Xna.Framework.Rectangle PlayerBoundary
		{
			get
			{
				return mPlayerBoundary;
			}
			set
			{
				mPlayerBoundary = value;
			}
		}

		public Microsoft.Xna.Framework.Rectangle VisibleBoundary
		{
			get
			{
				return mVisibleBoundary;
			}
			set
			{
				mVisibleBoundary = value;
			}
		}

		public int StartX
		{
			get
			{
				return mStartX;
			}
		}

		public int StartY
		{
			get
			{
				return mStartY;
			}
		}

		public Landscape Landscape
		{
			get
			{
				return mLandscape;
			}
		}

		public struct Sound
		{
			private SoundEffect mSoundEffect;
			private int mDisplacementX;
			private int mDisplacementY;

			public Sound(SoundEffect soundEffect, int x, int y)
			{
				mSoundEffect = soundEffect;
				mDisplacementX = x;
				mDisplacementY = y;
			}

			public int DisplacementX
			{
				get
				{
					return mDisplacementX;
				}
				set
				{
					mDisplacementX = value;
				}
			}

			public int DisplacementY
			{
				get
				{
					return mDisplacementY;
				}
				set
				{
					mDisplacementY = value;
				}
			}

			public SoundEffect SoundEffect
			{
				get
				{
					return mSoundEffect;
				}
				set
				{
					mSoundEffect = value;
				}
			}
		}
	}
}
