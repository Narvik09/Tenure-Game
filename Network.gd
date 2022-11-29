extends Node

const DEFAULT_PORT = 42042
const MAX_CLIENTS = 1

var server = null
var client = null

var ip_address = ""

func _ready() -> void:
	if OS.get_name() == "Android":
		ip_address = IP.get_local_addresses()[0]
	else:
		ip_address = IP.get_local_addresses()[3]
	
	for ip in IP.get_local_addresses():
		if ip.begins_with("127"):
		# if (ip.begins_with("192") or ip.begins_with("127") or ip.begins_with("10")):
			ip_address = ip
			
	get_tree().connect("connected_to_server", self, "_connected_to_server")
	get_tree().connect("server_disconnected", self, "_server_disconnected")
	
func create_server() -> void:
	server = NetworkedMultiplayerENet.new()
	server.create_server(DEFAULT_PORT, MAX_CLIENTS)
	get_tree().set_network_peer(server)
	print("Server created.")
	
func create_client() -> void:
	client = NetworkedMultiplayerENet.new()
	client.create_client(ip_address, DEFAULT_PORT)
	get_tree().set_network_peer(client)
	print("Client created.")
	
func _connected_to_server() -> void:
	print("Successfully connected to the server.")
	
func _server_disconnected() -> void:
	print("Disconnected from the server.")
