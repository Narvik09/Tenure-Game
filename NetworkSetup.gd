extends Control

onready var multiplayer_config_ui = $MultiplayerConfigure
onready var server_ip_address = $MultiplayerConfigure/ServerIPAddress

onready var device_ip_address = $CanvasLayer/DeviceIPAddress

func _ready():
	get_tree().connect("network_peer_connected", self, "_player_connected")
	get_tree().connect("network_peer_disconnected", self, "_player_disconnected")
	get_tree().connect("connected_to_server", self, "_connected_to_server")
	
	device_ip_address.text = Network.ip_address
	
func _player_connected(id) -> void:
	print("Player " + str(id) + " has connected.")
	
func _player_disconnected(id) -> void:
	print("Player " + str(id) + " has disconnected.")
	yield(get_tree().create_timer(2.0), "timeout")
	get_tree().change_scene("res://StartScreen.tscn")

func _on_CreateGame_pressed():
	multiplayer_config_ui.hide()
	Network.create_server()
	
	var main_network = load("res://MainNetwork.tscn")
	main_network = main_network.instance()
	add_child(main_network)

func _on_JoinGame_pressed():
	if (server_ip_address.text != ""):
		multiplayer_config_ui.hide()
		Network.ip_address = server_ip_address.text
		print("Creating a client connecting to IP address: " + Network.ip_address)
		Network.create_client()
		
		var main_network = load("res://MainNetwork.tscn")
		main_network = main_network.instance()
		add_child(main_network)


func _on_BackButton_pressed():
	get_tree().change_scene("res://StartScreen.tscn");
