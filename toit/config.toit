import .utils
import encoding.json
import .flespi-mqtt
import mqtt

class Config:
  static INSTANCE /Config? := null
  LAST_MOTOR_POSITION /int? := 0
  MAX_MOTOR_POSITION /int? := 1000
  MOTOR_REVERSED /bool := false

  constructor:
    get-config

  constructor.origin:
    if INSTANCE == null:
      Config
    return INSTANCE

  constructor.from_json json /Map:
    catch:
      LAST-MOTOR-POSITION = json["lastMotorPosition"]
      MAX-MOTOR-POSITION = json["maxMotorPosition"]
      MOTOR-REVERSED = json["motorReversed"]
      print "Config loaded; lastMotorPosition: $LAST-MOTOR-POSITION, maxMotorPosition: $MAX-MOTOR-POSITION"
      
  get-config:
    client /mqtt.Client := Flespi-MQTT.get-instance.get-client
    client.subscribe "$TOPIC-PREFIX/devices/$MAC/config" :: |topic /string payload /ByteArray|
      json := json.decode payload
      print "Received config message on topic: $topic with payload: $json"
      INSTANCE = Config.from-json json
      client.unsubscribe "$TOPIC-PREFIX/devices/$MAC/config"

    while INSTANCE == null:
      // get device MAC address and publish to devices topic
      client.publish "$TOPIC-PREFIX/devices" (json.encode {"mac":MAC}) --retain=true
      sleep --ms=10_000
    