using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.FreeGlut;
using Tao.OpenGl;

namespace GrafikprojektFraktalenGraphen
{
    class Program
    {
        static float lightness = 1.0f;
        static float deltalight = 0.8f;

        static int lineok;
        static int cx = 0, cy = 0, cz = 0;
        static int cn = 0;
        static float r = 0.2f, g = 0.1f, b = 0.05f;
        static int ok;
        static int cnl = 0, cng = 0, cnb = 0;
        static float angle2 = 0.0f, angle3 = 0.0f;
        static int moving, startx, starty, rotatexok, rotateyok;

        //Variables for Dijkstra
        private static int n, m, i, j, cost, k, k1, k2, ausgangsk, mini;
        private static int poz = 1;

        //Matrix for costs
        private static int[,] A = new int[50, 50];

        //Arrays for Distanz, Suchvektor und Vatervektor
        private static int[] D = new int[50];
        private static int[] S = new int[50];
        private static int[] TD = new int[50];

        //Way for every tree
        private static int[] wegfunc = new int[50];

        //How many trees the way has
        private static int wegtotalfunc=2;

        //STRUCT FOR EVERY TREE
        struct Tree {
            public int x, y, z;
            public float rtree, gtree, btree;
            public float rleaves, gleaves, bleaves;
            public int leavesok;
            public float angle;
            public int total;
            public int[] wegarray;
            public int wegtotal;
            

        }; static Tree[] T = new Tree[50];

        //STRUCT FOR EVERY LAKE
        struct Lake
        {
           public int lx, ly, lz;

        }; static Lake[] L = new Lake[50];

        //STRUCT FOR GRASS
        struct Grass
        {
            public int gx, gy, gz;

        }; static Grass[] G = new Grass[100];

        //STRUCT FOR BUSH
        struct Bush
        {
            public int bx, by, bz;

        }; static Bush[] B = new Bush[50];

        //Initialize tree, grass and bush according to cube coordinates
        private static void addCylinder()
        {
            cn++;
            T[0].total = cn;
            T[cn].x = cx;
            T[cn].y = cy;
            T[cn].z = cz;

            T[cn].rtree = r;
            T[cn].gtree = g;
            T[cn].btree = b;

            T[cn].leavesok = ok;
        }
        
        //Make the cylinder
        private static unsafe void MakeCylinder(float height,float Base)
        {
            Glu.GLUquadric obj;
            obj = Glu.gluNewQuadric();
            Gl.glPushMatrix();
            Gl.glRotatef(-90, 1.0f, 0.0f, 0.0f);
            Glu.gluCylinder(obj, Base, Base - (0.2 * Base), height, 20, 20);
            Gl.glPopMatrix();
        }

        //Add leaves to tree
        private static void leaves(float height, float lr, float lg, float lb)
        {
            Gl.glPushMatrix();
            Gl.glColor3f(lr, lg, lb);
            Glut.glutSolidCube(0.3f);
            Gl.glPopMatrix();
        }

        //Build the tree
        private static void maketree(float height, float Base, float angle, float r, float g, float b, float lr, float lg, float lb, int ok)
        {
            Gl.glPushMatrix();
            Gl.glPushAttrib(Gl.GL_LIGHTING_BIT);
            Gl.glColor3f(r,g,b);
            MakeCylinder(height, Base);
            Gl.glTranslatef(0.0f, height, 0.0f);

            if (height < 3 && ok != 0)
                leaves(height, lr, lg, lb);

            height -= height * 0.2f;                                      
            Base -= Base * 0.3f;

            if (height>1)
            {
                Gl.glPushMatrix();
                Gl.glRotatef(angle, -1.0f, 0.0f, 0.0f);                      //rotate to recursion angle, draw branch to the left
                maketree(height, Base, angle, r, g, b, lr, lg, lb, ok);          //recursive call
                Gl.glPopMatrix();

                Gl.glPushMatrix();
                Gl.glRotatef(angle, 0.5f, 0.0f, 0.866f);                     //rotate to recursion angle, draw branch to the right and back
                maketree(height, Base, angle, r, g, b, lr, lg, lb, ok);          //recursive call
                Gl.glPopMatrix();

                Gl.glPushMatrix();
                Gl.glRotatef(angle, 0.5f, 0.0f, -0.866f);                     //rotate to recursion angle, draw branch to the right and front
                maketree(height, Base, angle, r, g, b, lr, lg, lb, ok);           //recursive call
                Gl.glPopMatrix();
            }
            Gl.glPopMatrix();

            Gl.glPopAttrib();
        }
        
        //Add tree to scene
        private static void addTree()
        {
            int i;
            for (i=1;i<cn+1;i++)
            {
                Gl.glPushMatrix();
                Gl.glTranslatef(T[i].x, T[i].y, T[i].z);          //translate to tree coordinates

                //call maketree with respective angle, colors and leaves
                maketree(3.2f, 0.2f, T[i].angle, T[i].rtree, T[i].gtree, T[i].btree, T[i].rleaves, T[i].gleaves, T[i].bleaves, T[i].leavesok);
                Gl.glPopMatrix();
            }
        }

        //Initialize lake to cube coordinates
        private static void addLakeBase()
        {
            cnl++;
            L[cnl].lx = cx;
            L[cnl].ly = cy;
            L[cnl].lz = cz;
        }

        //Draw lake
        private static unsafe void makelake()
        {
            float color = 1.0f;
            double x = 0.0f;
            double y = 0.0f;
            float angle = 0.0f;
            float angle_stepsize = 0.1f;
            Glu.GLUquadric qobj1;
            qobj1 = Glu.gluNewQuadric();

            Gl.glPushMatrix();                              //draw cylinder
            Gl.glColor3f(0, 0, 1);
            Gl.glRotatef(-90, 1.0f, 0.0f, 0.0f);
            Glu.gluCylinder(qobj1, 5.0f, 5.0f, 0.1f, 20, 20);
            Gl.glPopMatrix();

            Gl.glRotatef(-90, 1.0f, 0.0f, 0.0f);          //draw circle on top of the cylinder
            Gl.glBegin(Gl.GL_POLYGON);
            angle = 0.0f;
            while (angle < 2 * 3.1415)
            {
                x = 5 * Math.Cos(angle);
                y = 5 * Math.Sin(angle);
                Gl.glColor3f(0, 0, color);
                color -= 0.01f;
                Gl.glVertex3f((float)x, (float)y, 0.1f);
                angle = angle + angle_stepsize;
            }
            Gl.glVertex3f(5, 0.0f, 0.1f);
            Gl.glEnd();
        }

        //Add lake to scene
        private static void addLake()
        {
            int i;
            for (i = 1; i < cnl + 1; i++)
            {
                Gl.glPushMatrix();
                Gl.glTranslatef(L[i].lx, L[i].ly, L[i].lz);
                makelake();
                Gl.glPopMatrix();
            }
        }

        //Initialize grass according to cube movement
        private static void addGrassbase()
        {
            cng++;
            G[cng].gx = cx;
            G[cng].gy = cy;
            G[cng].gz = cz;
        }

        //Build the grass
        private static void makegrass(float height, float Base, float angle)
        {
            Gl.glPushMatrix();
            Gl.glColor3f(0, 0.3f, 0);
            MakeCylinder(height, Base);                                   //make cylinder
            Gl.glTranslatef(0.0f, height, 0.0f);
            height -= height * 0.2f;                                      //make grass shorter
            Base -= Base * 0.3f;
            if (height > 0.2)
            {
                Gl.glPushMatrix();
                Gl.glRotatef(angle, -1.0f, 0.0f, 0.0f);
                makegrass(height, Base, angle);
                Gl.glPopMatrix();
            }
            Gl.glPopMatrix();
        }

        //Add grass to scene
        private static void addGrass()
        {
            int i;
            for (i = 1; i < cng + 1; i++)
            {
                Gl.glPushMatrix();
                Gl.glTranslatef(G[i].gx, G[i].gy, G[i].gz);
                makegrass(0.5f, 0.08f, 10);
                Gl.glPopMatrix();
                Gl.glPushMatrix();
                Gl.glTranslatef(G[i].gx, G[i].gy, G[i].gz);
                Gl.glRotatef(20, 0.5f, 0.0f, 0.866f);
                makegrass(0.5f, 0.08f, -10);
                Gl.glPopMatrix();
                Gl.glPushMatrix();
                Gl.glTranslatef(G[i].gx, G[i].gy, G[i].gz);
                Gl.glRotatef(20, 0.5f, 0.0f, -0.866f);
                makegrass(0.5f, 0.08f, 10);
                Gl.glPopMatrix();
            }
        }

        //Initialize bush according to cube movement
        private static void addBushBase()
        {
            cnb++;
            B[cnb].bx = cx;
            B[cnb].by = cy;
            B[cnb].bz = cz;
        }

        //Build the bush
        private static void makebush(float height, float Base, float angle)
        {
            Gl.glPushMatrix();

            Gl.glPushAttrib(Gl.GL_LIGHTING_BIT);
            Gl.glColor3f(0, 0.3f, 0);

            MakeCylinder(height, Base);
            Gl.glTranslatef(0.0f, height, 0.0f);

            Gl.glPushMatrix();
            Gl.glColor3f(0, 0.3f, 0);                            //color
            Glut.glutSolidCube(0.35);                               //1 leaf
            Gl.glPopMatrix();

            height -= height * 0.2f;                                      //make branch shorter
            Base -= Base * 0.3f;                                          //make branch thinner

            if (height > 0.5)                                               //make 3 branches
            {

                Gl.glPushMatrix();
                Gl.glRotatef(angle, -1.0f, 0.0f, 0.0f);                      //rotate to recursion angle, draw branch to the left
                makebush(height, Base, angle);          //recursive call
                Gl.glPopMatrix();

                Gl.glPushMatrix();
                Gl.glRotatef(angle, 0.5f, 0.0f, 0.866f);                     //rotate to recursion angle, draw branch to the right and back
                makebush(height, Base, angle);          //recursive call
                Gl.glPopMatrix();

                Gl.glPushMatrix();
                Gl.glRotatef(angle, 0.5f, 0.0f, -0.866f);                     //rotate to recursion angle, draw branch to the right and front
                makebush(height, Base, angle);           //recursive call
                Gl.glPopMatrix();
            }
            Gl.glPopMatrix();

            Gl.glPopAttrib();
        }

        //Add bush to scene
        private static void addBush()
        {
            int i;
            for (i = 1; i < cnb + 1; i++)
            {
                Gl.glPushMatrix();
                Gl.glTranslatef(B[i].bx, B[i].by, B[i].bz);          //translate to bush coordinates

                makebush(1.8f, 0.15f, 85);
                Gl.glPopMatrix();
            }
        }

        //Add cube to scene
        private static void theCube()
        {
            Gl.glPushMatrix();
            Gl.glColor3f(1, 1, 1);            //white
            Gl.glTranslatef(cx, cy, cz);      //translate to cube coordinates
            Glut.glutSolidCube(0.5);            //solid cube
            Gl.glPopMatrix();
        }

        //Draw scene
        private static void drawGrid()
        {
            
            Gl.glBegin(Gl.GL_POLYGON);
            Gl.glColor3f(0.0f, 0.001f, 0.0f);
            Gl.glVertex3f(19.0f, -0.1f, 0.0f);

            Gl.glColor3f(0.0f, 0.15f, 0.0f);
            Gl.glVertex3f(0.0f, -0.1f, 0.0f);

            Gl.glColor3f(0.0f, 0.1f, 0.0f);
            Gl.glVertex3f(0.0f, -0.1f, 19.0f);

            Gl.glColor3f(0.0f, 0.05f, 0.0f);
            Gl.glVertex3f(19.0f, -0.1f, 19.0f);

            Gl.glEnd();


        }

        //Rekursive Funktion in dem Vatervektor um den Weg zu finden
        private static void weg(int i)
        {
            if (TD[i] != 0)
            {
                weg(TD[i]);

                wegfunc[wegtotalfunc] = i;
                wegtotalfunc++;

            }
        }

        //Algorithmus von Dijkstra
        private static void Dijkstra()
        {
            Console.WriteLine("Anzahl Baume:");
            string num = Console.ReadLine();
            n = Int32.Parse(num);

            Console.WriteLine("Anzahl Kanten:");
            string num2 = Console.ReadLine();
            m = Int32.Parse(num2);

            for (i = 1; i <= m; i++)
            {
                Console.WriteLine("Kante " + i + " und Kosten: ");
                string line = Console.ReadLine();
                line = line.Trim();
                string[] tokens = line.Split(' ');
                int[] numbers = Array.ConvertAll(tokens, int.Parse);
                j = numbers[0];
                k = numbers[1];
                cost = numbers[2];

                A[j, k] = cost;
                A[k, j] = cost;
            }
            for (k1 = 1; k1 <= n; k1++)
                for (k2 = 1; k2 <= n; k2++)
                    if (A[k1, k2] == 0)
                        A[k1, k2] = 2000;

            Console.WriteLine("Ausgangsbaum: ");
            string num1 = Console.ReadLine();
            ausgangsk = Int32.Parse(num1);

            S[ausgangsk] = 1;
            for (i = 1; i <= n; i++)
            {
                D[i] = A[ausgangsk, i];
                if (i != ausgangsk)
                    if (D[i] != 2000)
                        TD[i] = ausgangsk;
            }

            //Suchen von Umwegen
            for (i = 1; i <= n; i++)
            {
                mini = 2000;
                for (j = 1; j <= n; j++)
                    if (S[j] == 0)
                        if (D[j] < mini)
                        {
                            mini = D[j];
                            poz = j;
                        }
                S[poz] = 1;
                for (j = 1; j <= n; j++)
                    if (S[j] == 0 && A[poz, j] != 2000)
                        if (D[j] > D[poz] + A[poz, j])
                        {
                            D[j] = D[poz] + A[poz, j];
                            TD[j] = poz;
                        }
            }
            

            for (int i = 1; i <= n; i++)
            {
                //Jeder Baum hat einen Weg vom Ausgangsbaum
                T[i].wegarray = new int[50];

                if (i != ausgangsk)
                {
                    //Im Weg Vektor werden Zahlen von der Position 2 eingefugt, das erste Element ist der Ausgangsbaum
                    wegtotalfunc = 2;
                    T[i].wegarray[1] = ausgangsk;

                    weg(i);
                    T[i].wegtotal = wegtotalfunc;
                    for (int s = 2; s<wegtotalfunc; s++)
                        T[i].wegarray[s] = wegfunc[s];
              
                }

            }
            
        }

        //Die Kosten und der Weg werden auf der Console geschrieben
        private static void writeDijkstra()
        {
            for (int i = 1; i <= n; i++)
            {
                if (i != ausgangsk)
                {
                    Console.WriteLine("Baum " + i + " Kosten: " + D[i]);
                    Console.WriteLine("Weg:");
                    for (int w = 1; w < T[i].wegtotal; w++)
                    {
                        Console.WriteLine(T[i].wegarray[w]);
                    }
                }
                Console.WriteLine("\n");
            }
        }

        //Dijkstra wird im Plan gezeichnet
       private static void drawDijkstra()
       {
           //Farbe jedes Weges
           float rd=0, gb=0, bd=0.5f;
           
           //Koordinaten der Linien (damit sie nicht ubereinstimmen)
           float mx=-0.5f, my=-0.5f, mz=-0.5f;

           for (int i = 1;  i<=n; i++)
           {
               if (lineok != 0 && i!=ausgangsk)
               {
                    for (int w=1; w<T[i].wegtotal-1; w++)
                    {
                        Gl.glEnable(Gl.GL_LINE_SMOOTH);
                        Gl.glBegin(Gl.GL_LINES);
                        Gl.glColor3f(rd, gb, bd);
                        Gl.glLineWidth(30);

                        //Es werden Linien gezeichnet zwischen zwei Baumen im Weg
                        Gl.glVertex3f(T[T[i].wegarray[w]].x+mx, T[T[i].wegarray[w]].y+my, T[T[i].wegarray[w]].z+mz);
                        Gl.glVertex3f(T[T[i].wegarray[w+1]].x+mx, T[T[i].wegarray[w+1]].y+my, T[T[i].wegarray[w+1]].z+mz);
                        Gl.glEnd();
                        Gl.glDisable(Gl.GL_LINE_SMOOTH);
                    }
                  

               }
                rd += 0.1f;
                mx += 0.5f;
                my += 0.5f;
                mz += 0.5f;
           }
       }

        private static void display()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glLoadIdentity();

            float [] LightDiffuse = { lightness+2.4f, lightness+2.4f, lightness+2.4f, 1.0f };
            float [] LightPosition = { 0.0f, 60.0f, 5.0f, 1.0f };
            float [] LightDirection = { 0.0f, 0.0f, -1.0f };
            float [] Material = { 0.5f, 0.5f, 0.5f, 1.0f };

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, LightDiffuse);

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, LightPosition);

            Gl.glLightf(Gl.GL_LIGHT0, Gl.GL_SPOT_CUTOFF, 60.0f);

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPOT_DIRECTION, LightDirection);

            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, Material);

            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glEnable(Gl.GL_LIGHTING);

            Gl.glTranslatef(0, 0, -45);  
            Gl.glRotatef(40, 1, 1, 0);

            if (rotateyok == 1)
            {
                Gl.glRotatef(angle2, 0, 1, 0);     //mouse rotate along y axis
                Gl.glRotatef(angle3, 0, 1, 0);
            }

            if (rotatexok == 1)
            {
                Gl.glRotatef(angle2, 1, 0, 0);     //mouse rotate along x axis
                Gl.glRotatef(angle3, 1, 0, 0);
            }

            drawGrid();                //call draw functions
            Gl.glTranslatef(-19, 0, 0);
            drawGrid();
            Gl.glTranslatef(0, 0, -19);
            drawGrid();
            Gl.glTranslatef(19, 0, 0);
            drawGrid();

            theCube();
            addTree();
            addLake();
            addGrass();
            addBush();
            drawDijkstra();
            
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_LIGHT0);
            Glut.glutSwapBuffers();
        }

        private static void init()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);

            Gl.glLoadIdentity();                       //initialize scene

            Glu.gluPerspective(35, 1.0f, 0.1f, 1000);   //set perspective

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1);
        }

        public static void keyboard(byte key, int x, int y)
        {
            //cube movement
            if (key == 119) //w
                cz -= 1; //forward
            if (key == 115) //s
                cz += 1; //backward
            if (key == 97) //a
                cx -= 1; //left
            if (key == 100) //d
                cx += 1;  //right
            if (key == 113) //q
                cy += 1; //up
            if (key == 122) //z
                cy -= 1; //down

            //add new tree
            if (key == 32) //spacebar
            {
                addCylinder();
            }

            //color of tree is red
            if (key == 110) //n
            {
                T[cn].rtree = 0.5f;    //red
                T[cn].gtree = 0;
                T[cn].btree = 0;
            }

            //color of tree is green
            if (key == 109) //m
            {
                T[cn].rtree = 0;    //green
                T[cn].gtree = 0.5f;
                T[cn].btree = 0;
            }

            //color of tree is blue
            if (key == 108) //l
            {
                T[cn].rtree = 0;    //blue
                T[cn].gtree = 0.5f;
                T[cn].btree = 0.5f;
            }

            //color of tree is brown
            if (key == 103) //g
            {
                T[cn].rtree = 0.2f;   //brown
                T[cn].gtree = 0.1f;
                T[cn].btree = 0.05f;
            }

            //increase angle manually
            if (key == 102) //f
            {
                T[cn].angle += 5;  
            }

            //decrease angle manually
            if (key == 101) //e
            {
                T[cn].angle -= 5;  
            }

            //add leaves
            if (key == 114) //r
            {
                T[cn].leavesok = 1;
            }

            //change color of leaves
            if (key == 116) //t
            {
                T[cn].rleaves = 1;  //white
                T[cn].gleaves = 1;
                T[cn].bleaves = 1;
            }

            if (key == 121) //y
            {
                T[cn].rleaves = 0;  //green
                T[cn].gleaves = 0.3f;
                T[cn].bleaves = 0;
            }

            //increase lightness
            if (key == 111) //o
            {
                lightness += deltalight;
            }

            //decrease lightness
            if (key == 105) //i
            {
                lightness -= deltalight;
            }

            //change scene rotation
            //rotate around y axis
            if (key == 104) //h
            {
                rotateyok = 1;
            }

            //stop rotation around y axis
            if (key == 106) //j
            {
                rotateyok = 0;
            }

            //rotate around x axis
            if (key == 98) //b
            {
                rotatexok = 1;
            }

            //stop rotation around x axis
            if (key == 118) //v
            {
                rotatexok = 0;
            }

            //add Dijkstra lines
            if (key==112) //p
            {
                lineok = 1;
            }

            //remove Dijkstra lines
            if (key==99) //c
            {
                lineok = 0;
            }

            //add lake to scene
            if (key==44) //,
            {
                addLakeBase();
            }

            //add grass to scene
            if (key==46) //.
            {
                addGrassbase();
            }

            //add bush to scene
            if (key==47) // /
            {
                addBushBase();
            }

            Glut.glutPostRedisplay();
        }

        //CAMERA MOVEMENT
        private static void mouse(int btn, int state, int x, int y)
        {
            if (btn == Glut.GLUT_LEFT_BUTTON && state == Glut.GLUT_DOWN) //if mouse pressed
            {
                moving = 1;
                startx = x;
                starty = y;
            }
            if (btn == Glut.GLUT_LEFT_BUTTON && state == Glut.GLUT_UP)
            {
                moving = 0;
            }
        }

        private static void motion(int x, int y)
        {
            if (moving==1)
            {
                angle2 = angle2 + (x - startx);
                angle3 = angle3 + (y - starty);

                startx = x;
                starty = y;
                Glut.glutPostRedisplay();
            }
        }

      

        static void Main(string[] args)
        {
            //Menu
            Console.WriteLine("MENU\n");
            Console.WriteLine("Keyboard commands:");

            Console.WriteLine("Cube movement:");
            Console.WriteLine("s for forward movement");
            Console.WriteLine("w for backward movement");
            Console.WriteLine("d for right movement");
            Console.WriteLine("a for left movement");
            Console.WriteLine("q for upward movement");
            Console.WriteLine("z for downward movement");

            Console.WriteLine("\nAdding elements: ");

            Console.WriteLine("Spcebar for new tree");
            Console.WriteLine("Tree settings:");
            Console.WriteLine("Change color:");
            Console.WriteLine("n for red");
            Console.WriteLine("m for green");
            Console.WriteLine("l for blue");
            Console.WriteLine("g for brown");
            Console.WriteLine("Change recursion angle:");
            Console.WriteLine("f for +5 degrees");
            Console.WriteLine("e for -5 degrees");
            Console.WriteLine("r for adding leaves");
            Console.WriteLine("Change leaves color:");
            Console.WriteLine("t for white");
            Console.WriteLine("y for green");

            Console.WriteLine("\n, for new lake");
            Console.WriteLine(". for grass");
            Console.WriteLine("/ for bush");

            Console.WriteLine("\np for Dijkstra distances");

            Console.WriteLine("\nChange scene settings:");
            Console.WriteLine("o for light increase");
            Console.WriteLine("i for light decrease");
            Console.WriteLine("h for rotation around y axis");
            Console.WriteLine("j for stopping rotation around y axis ");
            Console.WriteLine("b for rotation around x axis");
            Console.WriteLine("v for stopping rotation around x axis\n");

            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n");

            Dijkstra();
            writeDijkstra();
            Glut.glutInit();

            //display mode
            Glut.glutInitDisplayMode(Glut.GLUT_DEPTH | Glut.GLUT_DOUBLE | Glut.GLUT_RGBA);

            //window size, position, title
            Glut.glutInitWindowSize(800, 600);
            Glut.glutInitWindowPosition(600, 10);
            Glut.glutCreateWindow("Fraktalen Graphen");

            //call events

            Glut.glutDisplayFunc(display);
            Glut.glutKeyboardFunc(new Glut.KeyboardCallback(keyboard));
            init();

            Glut.glutMouseFunc(mouse);
            Glut.glutMotionFunc(motion);

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_NORMALIZE);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);

            //event processing loop
            Glut.glutMainLoop();
            
        }
    }
}
