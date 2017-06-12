/*
Codigo que envia el comando de apertura de la puerta por RF si recibe el string "Abrir" por serial
*/

#include <RCSwitch.h>

RCSwitch mySwitch = RCSwitch();
String incomingByte;   // for incoming serial data
void setup() 
{

	Serial.begin(9600);
	  
	// Transmitter is connected to Arduino Pin #10  
	mySwitch.enableTransmit(10);
	  
	//LED Integrado a la placa
	pinMode(LED_BUILTIN, OUTPUT);
	digitalWrite(LED_BUILTIN, LOW); 
	  

}

void loop() 

{ 
	digitalWrite(LED_BUILTIN, LOW); 
	if (Serial.available() > 0) 
	{
		//Leo del serial
		incomingByte = Serial.readString();
		  
		//Si me llega "abrir"  
		if (incomingByte == "Abrir")
		{
			//Envio cod de apertura de puerta	
			mySwitch.send(8059025,24);
			digitalWrite(LED_BUILTIN, HIGH); 
		}
    }
}



