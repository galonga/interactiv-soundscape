using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsPadSoundScape
{
    class GamePadController
    {
        // Das GamePad
        private Joystick joystick;
        public bool joystickAvable = false;
        // Status des GamePads
        private JoystickState state = new JoystickState();

        public GamePadController(DirectInput directInput, int number)
        {
            DirectInput input = new DirectInput();
            // Geräte suchen
            var devices = directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly);
            if (devices.Count == 0 || devices[number] == null)
            {
                // Kein Gamepad vorhanden
                return;
            }
            joystickAvable = true;
            // Gamepad erstellen
            joystick = new Joystick(directInput, devices[number].InstanceGuid);

            // Das GamePad soll nur reagieren, wenn sich unser Spiel(-fenster) im Vordergrund befindet
           // joystick.SetCooperativeLevel(game.Window.Handle, CooperativeLevel.Exclusive | CooperativeLevel.Foreground);

            // Den Zahlenbereich der Achsen auf -1000 bis 1000 setzen
            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-1000, 1000);

            }

            joystick.Acquire();
        }

        public JoystickState GetState()
        {
            if (joystick.Acquire().IsFailure || joystick.Poll().IsFailure)
            {
                // Wenn das GamePad nicht erreichbar ist, leeren Status zurückgeben.
                state = new JoystickState();
                return state;
            }

            state = joystick.GetCurrentState();

            return state;
        }

        public string GetControllerName()
        {
            if (joystickAvable)
                return joystick.Information.Type + " " + joystick.Information.ProductName + "; " + joystick.Information.ProductGuid;
            else
                return "no Joystick";
        }

        public void Release()
        {
            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
            joystick = null;
        }
    }
}
