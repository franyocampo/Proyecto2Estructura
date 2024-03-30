using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Linq;
using System.Collections;

namespace Proyecto2Juego
{
    public partial class Form1 : Form
    {
        private const int Filas = 6; //numero de FILAS en el tablero
        private const int Columnas = 7; //numero de COLUMNAS en el tablero
        private Queue<int> turnoJugador = new Queue<int>(); //cola para mantener el orden de los jugadores
        private int[,] tablero = new int[Filas, Columnas]; //tablero del juego representado como una matriz de tamaño FILAS x COLUMNAS
        private Stack<int> historialMovimientos = new Stack<int>();//PILA: historial de movimientos realizados en el juego
        private bool ganador = false; //indica si hay un ganador en el juego y inicializado como falso al inicio del juego
        private int partidasGanadasJugadorRojo = 0; //contador del marcador del jugador 1
        private int partidasGanadasJugadorAmarillo = 0; //contador del marcador del jugador 2
        private Queue<MouseEventArgs> historialPartidaClick = new Queue<MouseEventArgs>(); //cola para guardar la partida por clicks (progreso)
        private Queue<MouseEventArgs> historialPartidaGuardada = new Queue<MouseEventArgs>(); //cola para guardar la partida
        private bool jugadorInicial = false; //Para guardar el jugador inicial
        private bool jugadorGuardado = false;



        public Form1()
        {
            InitializeComponent();
            quienInicia();//acá se llama al evento QUIEN INICIA para que se genere el mensaje antes de que podamos mover fichas
            InicializarTableLayoutPanel();
            ActualizarMarcador(); //se llama al metodo para que siempre estén visibles los datos de los marcadores
        }
        private void quienInicia()//se crea este método para así establece la cola del turno 
        {
            //txtUltimaFicha.Text = "";
            turnoJugador = new Queue<int>(); //reinicia la cola
            //mostramos un mensaje preguntando al usuario qué jugador quiere que inicie primero
            DialogResult preguntaInicial = MessageBox.Show("¿Qué jugador empezará primero? Jugador ROJO (Sí) o Jugador AMARILLO (No)", "¿QUIÉN INICIA?", MessageBoxButtons.YesNo);

            //Dependiendo de la elección del usuario agregamos a los jugadores a la COLA en el orden que corresponde
            if (preguntaInicial == DialogResult.Yes)
            {
                turnoJugador.Enqueue(1);//Jugador 1 empieza primero
                turnoJugador.Enqueue(2);//Luego el Jugador 2
                jugadorInicial = true; //jugador rojo
            }
            else
            {
                turnoJugador.Enqueue(2);//Jugador 2 empieza primero
                turnoJugador.Enqueue(1);//Luego el Jugador 1
                jugadorInicial = false; //jugador amarillo
            }
        }


        private void InicializarTableLayoutPanel()
        {
            //limpiar las filas y columnas existentes
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();

            //agregamos estilos uniformes para las filas
            for (int i = 0; i < Filas; i++) //este bucle recorre todas las filas del TableLayoutPanel
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / Filas)); //se utilizan porcentajes para asegurarnos de que todas las filas tengan el mismo tamaño.
            }

            //agregamos estilos uniformes para las columnas
            for (int i = 0; i < Columnas; i++) //este bucle ecorre todas las columnas del TableLayoutPanel
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / Columnas));//divide el 100% por el número de columnas para asegurarnos de que todas las columnas tengan el mismo tamaño
            }
        }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            //obtenemos el número de filas y columnas en el TableLayoutPanel
            int fil = tableLayoutPanel1.RowCount; //fil son las filas
            int cols = tableLayoutPanel1.ColumnCount; //cols son Columnas

            //obtenemos las dimensiones de una celda dividiendo el ancho y alto del TableLayoutPanel entre el número de filas y columnas.
            int AnchoCelda = tableLayoutPanel1.Width / cols;
            int AlturaCelda = tableLayoutPanel1.Height / fil;

            //iterar sobre todas las celdas del TableLayoutPanel.
            for (int i = 0; i < fil; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //calculamos las coordenadas de la esquina superior izquierda de la celda actual
                    int x = j * AnchoCelda;
                    int y = (fil - 1 - i) * AlturaCelda;//acá dibujamos las fichas desde el lado inferior hacia el superior

                    //obtener el jugador en esta celda del tablero
                    int jugador = tablero[i, j];

                    //acá se elihe el color de la ficha basado en el jugador actual
                    Color color = jugador == 1 ? Color.Red : (jugador == 2 ? Color.Yellow : Color.White); //el color BLANCO es x si ninguno de los pasos anteriores es verdadero, significa que hay un error o una situación inesperada.
                                                                                                          //En este caso, se establece el color de la ficha en blanco

                    //se crea un pincel del color que necesitamos
                    Brush pincel = new SolidBrush(color);

                    //dibujamos un círculo en la celda actual usando el objeto Graphics proporcionado en los argumentos del evento Paint
                    e.Graphics.FillEllipse(pincel, x, y, AnchoCelda, AlturaCelda);
                }
            }
        }


        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            //verificamos si aún no hay un ganador
            if (!ganador)
            {
                int celdaVacia; //creamos esta variable para almacenar la fila vacía si es que hay


                //aqui es donde calculamos el ancho de cada columna dividiendo el ancho total del TableLayoutPanel por el número de columnas.   
                int cellWidth = tableLayoutPanel1.Width / Columnas; //tableLayoutPanel1.Width obtiene el ancho total del tablero,
                                                                    //el ancho se divide entre el número de columnas para determinar el ancho de cada columna.

                //e.X / cellWidth: se divide la posición horizontal del puntero del mouse (e.X) por el ancho de cada columna (cellWidth).
                //esto da como resultado el índice de la columna en la que se hizo clic.
                //la división entera se utiliza para asegurar que el resultado sea un numero entero que represente la columna.
                int ColumnaSeleccionada = e.X / cellWidth; //donde e.X representa la posicion horizontal del puntero del mouse en relacion con el control en el que se hizo clic.


                //aqui verificamos si la columna está llena
                if (columnaLlena(ColumnaSeleccionada, out celdaVacia))
                {
                    //si la columna está llena mostramos un un mensaje de advertencia 
                    MessageBox.Show("La columna está llena.");
                    return; //salir del método para evitar poner la ficha
                }

                //acá buscamos la fila vacía más baja en la columna que seleccionamos para así poner la ficha
                int fila = celdaVacia;

                //aqui se obtiene el jugador actual de la cola
                int jugadorActual = turnoJugador.Peek();

                // acá se dibuja una ficha roja o amarilla en la celda que seleccionamos
                Color color = jugadorActual == 1 ? Color.Red : Color.Yellow;
                dibujarFicha(ColumnaSeleccionada, fila, color);

                guardarClicks(e); //guarda el evento del mouse

                //actualizamos el tablero
                tablero[fila, ColumnaSeleccionada] = jugadorActual;

                //aca movemos al jugador actual de la cola y lo movemos al final 
                turnoJugador.Enqueue(turnoJugador.Dequeue());
                historialMovimientos.Push(jugadorActual);


                //llama al método para obtener las coordenadas y lo mostramos en el textbox del lugar donde colocamos la ultima ficha
                string coordenadas = ObtenerColumna(ColumnaSeleccionada); //se está llamando al método ObtenerCoordenadas para obtener las coordenadas de la última ficha colocada en el tablero
                                                                          //el método ObtenerCoordenadas toma dos parámetros que son fila y ColumnaSeleccionada, que representan la fila y la columna donde se colocó la última ficha.
                string coordenada = ObtenerFila(fila);

                //chequeamos si hay un ganador después de poner una ficha
                int colorGanador = comprobarGanador();

                //esto es para que se rellenen ambas filas primero antes de agregar una nueva fila
                bool primeraFilaCompleta = dgvHistorial.Rows.Count > 0 &&
                                           dgvHistorial.Rows[0].Cells[0].Value != null &&
                                           dgvHistorial.Rows[0].Cells[1].Value != null;

                //Si la primera fila ya está completa en ambas columnas, agregar una nueva fila
                if (primeraFilaCompleta)
                {
                    // Agregar una nueva fila al DataGridView
                    dgvHistorial.Rows.Insert(0, 1); // Insertar una nueva fila en la posición 0
                }

                //Obtener la fila actual (la primera agregada)
                int filaActual = 0;

                // Establecer el valor de la celda en la primera fila de la columna del jugador correspondiente
                if (jugadorActual == 1)
                {
                    dgvHistorial.Rows[filaActual].Cells[0].Value = $"Columna: {coordenadas} Fila: {coordenada}";
                }
                else if (jugadorActual == 2)
                {
                    dgvHistorial.Rows[filaActual].Cells[1].Value = $"Columna: {coordenadas} Fila: {coordenada}";
                }
                dgvHistorial.ClearSelection();

                if (colorGanador != 0)
                {
                    string nombreColor = colorGanador == 1 ? "Rojo" : "Amarillo"; // Obtener el nombre del color según el jugador ganador
                    //si hay un ganador actualizamos el contador
                    if (colorGanador == 1)
                    {
                        partidasGanadasJugadorRojo++;
                    }
                    else if (colorGanador == 2)
                    {
                        partidasGanadasJugadorAmarillo++;
                    }

                    //Actualizamos el marcador en la interfaz de usuario
                    ActualizarMarcador();
                    //si hay un ganador marcamos el juego como terminado y mostramos el mensaje del jugador ganador
                    ganador = true;
                    MessageBox.Show($" FELICIDADES! Jugador {nombreColor} ha ganado.");

                    limpiarPantalla();
                }
                else if (historialMovimientos.Count == Filas * Columnas) //verificamos si todas las celdas están llenas
                {

                    // Si todas las celdas están ocupadas y no hay un ganador mostrar un mensaje de que finaliza la partida
                    MessageBox.Show("Se acabó la partida, no tenemos ganador.");
                    nuevaPartida();
                    // Actualizar el marcador en la interfaz de usuario
                    ActualizarMarcador();


                }
            }
        }//cierre metodo

        private void dibujarFicha(int col, int filaa, Color color)
        {

            // Obtener las dimensiones de una celda
            int anchoCelda = tableLayoutPanel1.Width / Columnas;
            int alturaCelda = tableLayoutPanel1.Height / Filas;

            //se calculan las coordenadas de la esquina superior izquierda de la celda
            int x = col * anchoCelda;
            int y = (Filas - 1 - filaa) * alturaCelda; //calcular la coordenada y en función de la fila y de Rows

            //obtener el objeto Graphics del TableLayoutPanel
            using (Graphics g = tableLayoutPanel1.CreateGraphics())
            {
                //se crea un pincel del color especificado
                using (Brush pincel = new SolidBrush(color))
                {
                    //dibujar un círculo en la celda especificada
                    g.FillEllipse(pincel, x, y, anchoCelda, alturaCelda);
                }
            }

        }

        private bool columnaLlena(int col, out int CeldaVacia)
        {

            CeldaVacia = -1; //inicializamos la fila vacía como -1 indicando que no se encontró ninguna celda vacía

            //verificar si la columna está dentro del rango permitido
            if (col < 0 || col >= Columnas)
            {
                //si la columna está fuera del rango, se considera llena
                return true;
            }

            //recorrer todas las filas de la columna
            for (int filass = 0; filass < Filas; filass++)
            {
                //si encontramos una celda vacía en la columna, la columna no está llena
                if (tablero[filass, col] == 0)
                {
                    CeldaVacia = filass; // Actualizamos el índice de la fila vacía
                    return false;
                }
            }

            // Si no encontramos ninguna celda vacía, la columna está llena
            return true;
        }


        private int comprobarGanador()
        {
            // Verificar si hay cuatro fichas conectadas horizontalmente
            for (int f = 0; f < Filas; f++) // f es la fila
            {
                for (int c = 0; c <= Columnas - 4; c++) //c son columnas
                {
                    // Comprobar si la casilla no está vacía y si las siguientes tres casillas en la misma fila son iguales
                    if (tablero[f, c] != 0 &&
                        tablero[f, c] == tablero[f, c + 1] &&
                        tablero[f, c] == tablero[f, c + 2] &&
                        tablero[f, c] == tablero[f, c + 3])
                    {

                        return tablero[f, c]; // Retorna el número del jugador ganador

                    }

                }

            }



            // Verificar si hay cuatro fichas conectadas verticalmente
            for (int c = 0; c < Columnas; c++)
            {
                for (int f = 0; f <= Filas - 4; f++)
                {
                    // Comprobar si la casilla no está vacía y si las siguientes tres casillas en la misma columna son iguales
                    if (tablero[f, c] != 0 &&
                        tablero[f, c] == tablero[f + 1, c] &&
                        tablero[f, c] == tablero[f + 2, c] &&
                        tablero[f, c] == tablero[f + 3, c])
                    {
                        return tablero[f, c]; // Retorna el número del jugador ganador
                    }
                }
            }

            // Verificar si hay cuatro fichas conectadas diagonalmente (hacia la derecha)
            for (int f = 0; f <= Filas - 4; f++)
            {
                for (int c = 0; c <= Columnas - 4; c++)
                {
                    // Comprobar si la casilla no está vacía y si las siguientes tres casillas en diagonal hacia la derecha son iguales
                    if (tablero[f, c] != 0 &&
                        tablero[f, c] == tablero[f + 1, c + 1] &&
                        tablero[f, c] == tablero[f + 2, c + 2] &&
                        tablero[f, c] == tablero[f + 3, c + 3])
                    {
                        return tablero[f, c]; // Retorna el número del jugador ganador
                    }
                }

            }


            // Verificar si hay cuatro fichas conectadas diagonalmente (hacia la izquierda)
            for (int f = 0; f <= Filas - 4; f++)
            {
                for (int c = 3; c < Columnas; c++)
                {
                    // Comprobar si la casilla no está vacía y si las siguientes tres casillas en diagonal hacia la izquierda son iguales
                    if (tablero[f, c] != 0 &&
                        tablero[f, c] == tablero[f + 1, c - 1] &&
                        tablero[f, c] == tablero[f + 2, c - 2] &&
                        tablero[f, c] == tablero[f + 3, c - 3])
                    {
                        return tablero[f, c]; // Retorna el número del jugador ganador
                    }
                }
            }

            return 0; // No hay un ganador

        }

        private void nuevaPartida() //evento encargado de reiniciar el juego
        {

            // Limpiar el tablero y el historial de movimientos
            for (int i = 0; i < Filas; i++)
            {
                for (int j = 0; j < Columnas; j++)
                {
                    tablero[i, j] = 0;
                }
            }
            historialMovimientos.Clear();

            //Reiniciamos el juego
            ganador = false;

            dgvHistorial.Rows.Clear();  //limpiamos todas las filas
            quienInicia();//vuelve a preguntar quien juega primero

            // Volver a dibujar el tablero
            tableLayoutPanel1.Invalidate();

        }
        private void limpiarPantalla()//método para limpiar pantalla y si no reinicia el juego entonces sigue jugando con el mismo jugador que inició partida

        {
            for (int i = 0; i < Filas; i++)
            {
                for (int j = 0; j < Columnas; j++)
                {
                    tablero[i, j] = 0;
                }
            }
            historialMovimientos.Clear();
            ganador = false;
            //volver a dibujar el tablero
            tableLayoutPanel1.Invalidate();
            dgvHistorial.Rows.Clear();  //limpiamos todas las filas
        }


        private void ActualizarMarcador()
        {
            // Actualizar el texto del marcador con el puntaje actualizado
            txtJugador1.Text = $"Jugador Rojo: {partidasGanadasJugadorRojo}";
            txtJugador2.Text = $"Jugador Amarillo: {partidasGanadasJugadorAmarillo}";

        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //If paraa confirmar salida
            if (MessageBox.Show("¿Desea salir del sistema?", " Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit(); //se finaliza la app
            }
        }


        private string ObtenerColumna(int columna)
        {
            char letraColumna = (char)('A' + columna); //convertir el número de columna a letra

            return $"{letraColumna}"; //combinar letra de columna
        }
        private string ObtenerFila(int fila)
        {
            int numeroFila = Filas - fila; //invertimos el número de fila ya que generalmente se cuenta desde arriba

            return $"{numeroFila}";
        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnReinciar_Click_1(object sender, EventArgs e) // BOTON DE REINICIAR 
        {
            nuevaPartida();//acá se llama al método de reiniciar
            historialPartidaClick.Clear();
        }


        private void btnReproducir_Click(object sender, EventArgs e)
        {
            SoundPlayer Sonido = new SoundPlayer();
            Sonido.SoundLocation = "C://Users//ariel//OneDrive//Documentos//Programacion//Estructura de Datos - 2024 I cuatri//Proyecto2 guardado cargado//Proyecto2Juego//proyecto 4 en linea//musica_4EnLinea//Last_Stop.wav";
            Sonido.Play();
        }

        private void Detener_Click(object sender, EventArgs e)
        {
            SoundPlayer Sonido = new SoundPlayer();
            Sonido.SoundLocation = "C://Users//ariel//OneDrive//Documentos//Programacion//Estructura de Datos - 2024 I cuatri//Proyecto2 guardado cargado//Proyecto2Juego//proyecto 4 en linea//Last_Stop.wav";
            Sonido.Stop();
        }

        private void dgvHistorial_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void nuevaPartidaCargada() //evento encargado de reiniciar el juego
        {

            // Limpiar el tablero y el historial de movimientos
            for (int i = 0; i < Filas; i++)
            {
                for (int j = 0; j < Columnas; j++)
                {
                    tablero[i, j] = 0;
                }
            }
            historialMovimientos.Clear();

            //Reiniciamos el juego
            ganador = false;

            dgvHistorial.Rows.Clear();  //limpiamos todas las filas

            // Volver a dibujar el tablero
            tableLayoutPanel1.Invalidate();

        }

        private void guardarClicks(MouseEventArgs e) //evento que guarda clicks ya que el juego se basa en clicks
        {
            historialPartidaClick.Enqueue(e); //agregar los movimientos o clicks al Queue

        }

        private void pictureBox6_Guardar_Click(object sender, EventArgs e)
        {
            nuevaPartidaCargada();
            historialPartidaGuardada = historialPartidaClick; //guardar el progreso en una partida 

            jugadorGuardado = jugadorInicial; //se almacenan estan variables para un control de los jugadores que se guardaron sin importar la pregunta inicial
            historialPartidaClick = new Queue<MouseEventArgs>(); //Se reinicia el Queue

            MessageBox.Show("Partida guardada correctamente. \nSe ha reiniciado, cargue partida.");
        }

        private void pictureBox7_Cargar_Click(object sender, EventArgs e)
        {

            //limpiar pantalla antes de cargar partida
            nuevaPartidaCargada();

            turnoJugador = new Queue<int>();
            jugadorInicial = jugadorGuardado;   //se usa este metodo para no se reinicien los jugadores durante las cargas
                                                //se inicia con rojo y cuando carga siempre sera el jugador rojo
            if (jugadorGuardado)
            {
                turnoJugador.Enqueue(1);//Jugador 1 empieza primero
                turnoJugador.Enqueue(2);//Luego el Jugador 2
            }
            else
            {
                turnoJugador.Enqueue(2);//Jugador 2 empieza primero
                turnoJugador.Enqueue(1);//Luego el Jugador 1
            }


            int cantidadElementos = historialPartidaGuardada.Count; // Almacenar la cantidad original de elementos

            Queue<MouseEventArgs> temporalQueue = new Queue<MouseEventArgs>(historialPartidaGuardada); //Se crea Queue temporal por cuestion de integridad de los datos

            for (int i = 0; i < cantidadElementos; i++)
            {
                MouseEventArgs evento = temporalQueue.Dequeue(); //Se usa Queue temporal por cuestion de integridad de los datos

                tableLayoutPanel1_MouseClick(null, evento); // Invocar el método con el evento que modifica los clicks (el progreso)

            }

        }




    }//cierre form

}//cierre namespace
