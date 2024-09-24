using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    //UI Containers
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    //lobby text to change
    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    //extesible lists to hold a changing number of room buttons
    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    private void Start()
    {
        //disable menus that aren't start
        mainScreen.SetActive(true);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        //disable buttons until connected
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        //make sure the cursor is free as it is locked during gameplay
        Cursor.lockState = CursorLockMode.None;

        //if in room from previous game
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    /// <summary>
    /// disables all screens then renables passed screen
    /// </summary>
    /// <param name="screen"></param>
    private void SetScreen(GameObject screen)
    {
        
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        screen.SetActive(true);

        if(screen == lobbyBrowserScreen)
        {
            UpdateLobbyBrowserUI();
        }
    }

    /*************************
     * MAIN SCREEN FUNCTIONS *
     *************************/
    /// <summary>
    /// changes photon nickname to what is in the input field
    /// </summary>
    /// <param name="playerNameInput"></param>
    public void OnPlayerNameChanged(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    //allows the player to interact with buttons when they have connected to photon
    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }

    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }

    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);
    }

    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }

    /// <summary>
    /// creates a room with the name of the input
    /// </summary>
    /// <param name="roomNameInput"></param>
    public void OnCreateButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }
    
    /**************************
     * LOBBY SCREEN FUNCTIONS *
     **************************/

    //ask Slease about override keyword
    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    [PunRPC]
    private void UpdateLobbyUI()
    {
        //only masterclient can start game
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        //iterates through the player nicknames from photon
        playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        roomInfoText.text = PhotonNetwork.CurrentRoom.Name;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public void OnStartGameButton()
    {
        //tells photon that the room can't be joined
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        //rpc the scene change
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    /************************
     * LOBBY BROWSER SCREEN *
     ************************/

    private void UpdateLobbyBrowserUI()
    {
        foreach (GameObject button in roomButtons)
        {
            button.SetActive(false);
        }

        for(int x = 0; x < roomList.Count; x++)
        {
            //lambda calc
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];
            button.SetActive(true);
            button.transform.Find("RoomName").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            //action on click
            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); }); //what is this, probably makes it so it triggers on click but like what
        }
    }

    /// <summary>
    /// instatiates a button and puts it in the correct place in the outliner
    /// </summary>
    /// <returns></returns>
    private GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);
        return buttonObj;
    }

    public void OnJoinRoomButton(string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }

    //this is so dumb
    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        roomList = allRooms;
    }
}
