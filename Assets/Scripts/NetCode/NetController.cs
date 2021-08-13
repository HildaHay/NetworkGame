using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using System;

namespace MeowyServerEvent {
    [Serializable]
    public class Event {
        public string type;

        public static Event DeserializeFromJSON(string json_str) {
			Event raw_event = JsonUtility.FromJson<Event>(json_str);
			switch(raw_event.type){
				case "player_added" :
					return JsonUtility.FromJson<PlayerAdded>(json_str);
				case "player_move" :
					return JsonUtility.FromJson<PlayerMove>(json_str);
				case "player_disconnected" :
					return JsonUtility.FromJson<PlayerDisconnected>(json_str);
				default:
					return raw_event;
			}
        }
		public virtual void handle(NetController network_controller){}
    }

	[Serializable]
	public class PlayerEvent : Event {
		public string player_name;
	}

	// Sent when new player is added to game
	[Serializable]
	public class PlayerAdded : PlayerEvent {

		public override void handle(NetController network_controller){
			network_controller.add_player(this.player_name);
		}
	}

	// Sent whenever a player moves on their device.
	[Serializable]
	public class PlayerMove : PlayerEvent {
		public float pointer_x;
		public float pointer_y;

		public override void handle(NetController network_controller){
			network_controller.player_move(this.player_name, this.pointer_x, this.pointer_y);
		}
	}

	// Sent whenever a player is disconnected because of inactivity/broken socket.
	[Serializable]
	public class PlayerDisconnected : PlayerEvent {
		public override void handle(NetController network_controller){
			network_controller.player_disconnected(this.player_name);
		}
	}
}
public class NetController : MonoBehaviour {

	[Tooltip("The players which are added to the game when a person joins")]
	public GameObject playerPrefab;

	public Dictionary<string, PlayerSoul> players = new Dictionary<string, PlayerSoul>();

	[Tooltip("The Animation to call on game over.")]
	public Animator cameraAnimator;

	void Awake() {
		DontDestroyOnLoad(this);
	}

    // Start is called before the first frame update
    void Start() {
        startServer();
    }

    // Update is called once per frame
    void Update() {
		checkManualControllers();
        processNetworkMessages();
    }

	public GameObject add_player(string player_name, bool localControls=false) {
		Debug.Log("Player Connected: " + player_name);
		Vector3 randomPos = new Vector3(10, UnityEngine.Random.Range(-1f, 1f), 5);
		GameObject playerObject = Instantiate(playerPrefab, randomPos, Quaternion.identity) as GameObject;
		playerObject.name = player_name;

		PlayerSoul playBoi = playerObject.GetComponent<PlayerSoul>();

		players.Add(player_name, playBoi);

		

		return playerObject;
	}

	public void player_move(string player_name, float pointer_x, float pointer_y) {
		Debug.Log("Player Moving: " + player_name);
	}

	// TODO: Proper disconnection logic.
	public void player_disconnected(string player_name) {
		Debug.Log("Player Disconnected: " + player_name);
	}

	public void OnDestroy()
	{
		networkThread.Abort();
		stream.Close();
	}

	public static void send(NetworkMessage msg) {
		msg.WriteToStream(writer);
		writer.Flush();
	}

	// Runs for all non-networked controllers.
	private static List<String> inputStringsVertical = new List<String>{ "VerticalArrow", "VerticalWASD" };
	private static List<String> inputStringsHorizontal = new List<String> { "HorizontalArrow", "HorizontalWASD" };
	private void checkManualControllers()
	{
		for (int i=0; i<inputStringsHorizontal.Count; i++)
		{
			if (Input.GetAxisRaw(inputStringsVertical[i]) > 0.5)
			{
				GameObject curPlayer = add_player(inputStringsVertical[i], true);
				KeysToVirtual keys = curPlayer.AddComponent<KeysToVirtual>();
				keys.horizontalString = inputStringsVertical[i];
				keys.verticalString = inputStringsHorizontal[i];
				keys.toBeControlled = curPlayer.GetComponent<PlayerCharacter>();

				inputStringsHorizontal.RemoveAt(i);
				inputStringsVertical.RemoveAt(i);
			}
		}
	}

	private static TcpClient client = null;
	private static BinaryReader reader = null;
	private static BinaryWriter writer = null;
	private static Thread networkThread = null;
	private static Queue<NetworkMessage> messageQueue = new Queue<NetworkMessage>();

	private static void addItemToQueue(NetworkMessage item) {
		lock(messageQueue) {
			messageQueue.Enqueue(item);
		}
	}

	private static NetworkMessage getItemFromQueue() {
		lock(messageQueue) {
			if (messageQueue.Count > 0) {
				return messageQueue.Dequeue();
			} else {
				return null;
			}
		}
	}

	private void processNetworkMessages() {
		NetworkMessage msg = getItemFromQueue();
		while (msg != null) {
			MeowyServerEvent.Event networkevent = MeowyServerEvent.Event.DeserializeFromJSON(msg.ToString());
			networkevent.handle(this);

			msg = getItemFromQueue();
		}
	}

	private static void startServer() {
		Debug.Log("Attempting to start server...");
		if (networkThread == null) {
			connect();
			networkThread = new Thread(() => {
					Debug.Log("NetworkThread starting...");
					while (reader != null) {
						NetworkMessage msg = NetworkMessage.ReadFromStream(reader);
						addItemToQueue(msg);
					}
					lock(networkThread) {
						networkThread = null;
					}
				});
			networkThread.Start();
		}
	}

	private static Stream stream;

	private static void connect() {
		if (client == null) {
			string server = "localhost";
			int port = 8002;
			client = new TcpClient(server, port);
			stream = client.GetStream();
			reader = new BinaryReader(stream);
			writer = new BinaryWriter(stream);
		}
	}
}
