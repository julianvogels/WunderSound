#include <SoftwareSerial.h>
#include <WunderbarBridge.h>
#define BRIDGE_DEBUG true

const int DEBUG_TX = 21;
const int DEBUG_RX = 22;
/*  Config the bridge module, the 3rd parameter is the baudrate,
    which has to be the same used in the PC serial monitor application*/
const int LED1=13;
const int LED2=12;
Bridge bridge = Bridge(DEBUG_RX, DEBUG_TX, 115200); 

// the serialEvent() handler is called on every received data 
//from the serial port. */
/*void serialEvent(){
	bridge.processSerial();
}*/

void setup()   
{                
  pinMode(LED1, OUTPUT); 
  pinMode(LED2, OUTPUT); 
//  bridge.begin();
  Serial.begin(115200);  
}

void loop()                     
{
     //   static uint8_t dataOut[1] = {1};
//	static bridge_payload_t rxPayload;
/*	if (bridge.newData == true)
{
        	bridge.sendData(dataOut, sizeof(dataOut));
		rxPayload = bridge.getData();
                Serial.write(rxPayload.payload[0]);
                if (rxPayload.payload[0]==1)
                digitalWrite(LED1,HIGH);
                else if (rxPayload.payload[0]==0)
                digitalWrite(LED1,LOW);
             //   sendData(rxPayload, sizeof(rxPayload));
		digitalWrite(LED2, HIGH);
              }*/

  if(Serial.available())
  {
    int c = Serial.read();
    if (c == '1')
    {    
      digitalWrite(LED1,HIGH);
      Serial.write("on");
      delay(100);
    }
   else if (c == '0')
    {
      digitalWrite(LED1,LOW);
      Serial.write("off");
      delay(100);
    }
   else if (c == '2')
    {
      digitalWrite(LED2,HIGH);
      Serial.write("on");
      delay(100);
    }
   else if (c == '9')
    {
      digitalWrite(LED2,LOW);
      Serial1.write("off");
      delay(100);
    }
    
  }
}


