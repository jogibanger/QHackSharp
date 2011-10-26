﻿namespace HackSharp
{
    internal class Dungeon
    {
        public static byte existence_chance;
        public static byte[,] map = new byte[Config.MAP_W,Config.MAP_H];
        public DungeonComplex d = new DungeonComplex();


        //Define all the basic dungeon structures and create the complete dungeon map.
        public void init_dungeon()
        {
            create_complete_dungeon();
        }

        /// <summary>
        /// Create all the levels in the dungeon.
        /// </summary>
        private void create_complete_dungeon()
        {
            for (d.dl = 0; d.dl < Config.MAX_DUNGEON_LEVEL; d.dl++)
            {
                /* Basic initialization. */

                /* Nothing is known about the dungeon at this point. */
                for (byte x = 0; x < Config.MAP_W; x++)
                    for (byte y = 0; y < Config.MAP_H; y++)
                        set_knowledge(x, y, 0);

                /* Create the current level map. */
                dig_level();

                /* Note the current level as unvisited. */
                d.visited[d.dl] = false;
            }
        }

        /// <summary>
        /// Create one single dungeon level.
        /// </summary>
        private void dig_level()
        {
            byte w;
            byte h;
            var sectx = new byte[Config.SECT_NUMBER];
            var secty = new byte[Config.SECT_NUMBER];
            var index = new short[Config.SECT_NUMBER];

            // Determine a random order for the section generation.
            /* Initial order. */
            short i = 0;
            for (w = 0; w < Config.NSECT_W; w++)
                for (h = 0; h < Config.NSECT_H; h++)
                {
                    index[i] = i;
                    sectx[i] = w;
                    secty[i] = h;
                    i++;
                }

            /* Randomly shuffle the initial order. */
            for (i = 0; i < Config.SECT_NUMBER; i++)
            {
                short j = rand_int(Config.SECT_NUMBER);
                short k = rand_int(Config.SECT_NUMBER);
                short dummy = index[j];
                index[j] = index[k];
                index[k] = dummy;
            }
            //Create each section separately.

            /* Initially there is a 30% chance for rooms to be non-existant. */
            existence_chance = 70;

            /* Dig each section. */
            for (i = 0; i < Config.SECT_NUMBER; i++)
                dig_section(sectx[index[i]], secty[index[i]]);

            /* Build some stairs. */
            dig_stairs();
        }

        private void dig_section(byte x, byte y)
        {
            if (rand_byte(100) + 1 >= existence_chance)
            {
                /* No room here. */
                d.s[d.dl, x, y].exists = false;

                /* Decrease the chance for further empty rooms. */
                existence_chance += 3;
            }
            else
            {
                byte dir;

                /* Yeah :-) ! */
                d.s[d.dl, x, y].exists = true;

                /*
                 * Dig a room.
                 *
                 * Rooms are at least 4x4 tiles in size.
                 */

                do
                {
                    d.s[d.dl, x, y].rx1 = x*Config.SECT_W + rand_byte(3) + 1;
                    d.s[d.dl, x, y].ry1 = y*Config.SECT_H + rand_byte(3) + 1;
                    d.s[d.dl, x, y].rx2 = (x + 1)*Config.SECT_W - rand_byte(3) - 2;
                    d.s[d.dl, x, y].ry2 = (y + 1)*Config.SECT_H - rand_byte(3) - 2;
                } while (d.s[d.dl, x, y].rx2 - d.s[d.dl, x, y].rx1
                         < 3 ||
                         d.s[d.dl, x, y].ry2 - d.s[d.dl, x, y].ry1
                         < 3);

                /*
                 * Create doors.
                 *
                 * XXX: At some point it would be nice to create doors only for
                 *      some directions to make the dungeon less regular.
                 */

                for (dir = (byte) Direction.N; dir <= (byte) Direction.E; dir++)
                    if (dir_possible(x, y, dir))
                    {
                        switch ((Direction) dir)
                        {
                            case Direction.N:
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1;
                                break;

                            case Direction.S:
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1 + rand_byte(room_width(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry2;
                                break;

                            case Direction.E:
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx2;
                                break;

                            case Direction.W:
                                d.s[d.dl, x, y].dy[dir] = d.s[d.dl, x, y].ry1 + rand_byte(room_height(x, y) - 1) + 1;
                                d.s[d.dl, x, y].dx[dir] = d.s[d.dl, x, y].rx1;
                                break;
                        }
                        d.s[d.dl, x, y].dt[dir] = rand_door();
                    }
                    else
                        d.s[d.dl, x, y].dt[dir] = Tiles.NO_DOOR;
            }
        }

        /// <summary>
        /// Calculate the room width for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int room_width(byte x, byte y)
        {
            return (d.s[d.dl, x, y].rx2 - d.s[d.dl, x, y].rx1 - 1);
        }

        /// <summary>
        /// Calculate the room height for a specific room section at (x, y).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int room_height(byte x, byte y)
        {
            return (d.s[d.dl, x, y].ry2 - d.s[d.dl, x, y].ry1 - 1);
        }

        /// <summary>
        /// Determine a random door type.
        /// </summary>
        /// <returns></returns>
        private byte rand_door()
        {
            byte roll = rand_byte(100);

            if (roll < 75)
                return (byte) Tiles.OPEN_DOOR;
            else if (roll < 90)
                return (byte) Tiles.CLOSED_DOOR;

            return (byte) Tiles.LOCKED_DOOR;
        }

        /// <summary>
        /// Build a map for the current dungeon level.
        /// 
        /// This function is very important for QHack in general.  Levels are only
        /// stored by their section descriptions.  The actual map is created when the
        /// level is entered.  The positive thing about this is that it requires much
        /// less space to save a level in this way (since you only need the outline
        /// descriptions).  The negative thing is that tunneling and other additions
        /// are not possible since the level desciptions have now way of recording
        /// them.
        /// </summary>
        private void build_map()
        {
            int x;
            int y;
            byte sx;
            byte sy;

            /* Basic initialization. */
            for (x = 0; x < Config.MAP_W; x++)
                for (y = 0; y < Config.MAP_H; y++)
                    map[x, y] = (byte) Tiles.ROCK;

            /* Build each section. */
            for (sx = 0; sx < Config.NSECT_W; sx++)
                for (sy = 0; sy < Config.NSECT_H; sy++)
                {
                    /* Handle each section. */
                    if (d.s[d.dl, sx, sy].exists)
                    {
                        /* Paint existing room. */
                        for (x = d.s[d.dl, sx, sy].rx1 + 1;
                             x < d.s[d.dl, sx, sy].rx2;
                             x++)
                            for (y = d.s[d.dl, sx, sy].ry1 + 1;
                                 y < d.s[d.dl, sx, sy].ry2;
                                 y++)
                                map[x, y] = (byte) Tiles.FLOOR;

                        /* Paint doors. */
                        byte dir;
                        for (dir = (byte) Direction.N; dir <= (byte) Direction.E; dir++)
                            if (d.s[d.dl, sx, sy].dt[dir] != Tiles.NO_DOOR)
                                map[d.s[d.dl, sx, sy].dx[dir], d.s[d.dl, sx, sy].dy[dir]]
                                    = d.s[d.dl, sx, sy].dt[dir];
                    }
                }


            /* Connect each section. */
            for (sx = 0; sx < Config.NSECT_W; sx++)
                for (sy = 0; sy < Config.NSECT_H; sy++)
                {
                    if (dir_possible(sx, sy, Direction.E))
                        connect_sections(sx, sy, sx + 1, sy, (int) Direction.E);
                    if (dir_possible(sx, sy, Direction.S))
                        connect_sections(sx, sy, sx, sy + 1, (int) Direction.S);
                }

            /* Place the stairways. */
            map[d.stxu[d.dl], d.styu[d.dl]] = (byte) Tiles.STAIR_UP;
            if (d.dl < Config.MAX_DUNGEON_LEVEL - 1)
                map[d.stxd[d.dl], d.styd[d.dl]] = (byte) Tiles.STAIR_DOWN;
        }


        /// <summary>
        /// Connect two sections of a level.
        /// </summary>
        /// <param name="sx1"></param>
        /// <param name="sy1"></param>
        /// <param name="sx2"></param>
        /// <param name="sy2"></param>
        /// <param name="dir"></param>
        private void connect_sections(int sx1, int sy1, int sx2, int sy2, int dir)
        {
            int cx1;
            int cy1;
            int cx2;
            int cy2;

            /* Get the start byteinates from section #1. */
            if (d.s[d.dl, sx1, sy1].exists)
            {
                if (dir == (byte) Direction.S)
                {
                    cx1 = d.s[d.dl, sx1, sy1].dx[(byte) Direction.S];
                    cy1 = d.s[d.dl, sx1, sy1].dy[(byte) Direction.S];
                }
                else
                {
                    cx1 = d.s[d.dl, sx1, sy1].dx[(byte) Direction.E];
                    cy1 = d.s[d.dl, sx1, sy1].dy[(byte) Direction.E];
                }
            }
            else
            {
                cx1 = sx1*Config.SECT_W + (Config.SECT_W/2);
                cy1 = sy1*Config.SECT_H + (Config.SECT_H/2);
            }

            /* Get the end byteinates from section #2. */
            if (d.s[d.dl, sx2, sy2].exists)
            {
                if (dir == (byte) Direction.S)
                {
                    cx2 = d.s[d.dl, sx2, sy2].dx[(byte) Direction.N];
                    cy2 = d.s[d.dl, sx2, sy2].dy[(byte) Direction.N];
                }
                else
                {
                    cx2 = d.s[d.dl, sx2, sy2].dx[(byte) Direction.W];
                    cy2 = d.s[d.dl, sx2, sy2].dy[(byte) Direction.W];
                }
            }
            else
            {
                cx2 = sx2*Config.SECT_W + (Config.SECT_W/2);
                cy2 = sy2*Config.SECT_H + (Config.SECT_H/2);
            }

            /* Get the middle of the section. */
            int mx = (cx1 + cx2)/2;
            int my = (cy1 + cy2)/2;

            /* Draw the tunnel. */
            int x = cx1;
            int y = cy1;
            if (dir == (byte) Direction.E)
            {
                /* Part #1. */
                while (x < mx)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    x++;
                }

                /* Part #2. */
                if (y < cy2)
                    while (y < cy2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        y++;
                    }
                else
                    while (y > cy2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        y--;
                    }

                /* Part #3. */
                while (x < cx2)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    x++;
                }
                if (map[x, y] == (byte) Tiles.ROCK)
                    map[x, y] = (byte) Tiles.FLOOR;
            }
            else
            {
                /* Part #1. */
                while (y < my)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    y++;
                }
                if (map[x, y] == (byte) Tiles.ROCK)
                    map[x, y] = (byte) Tiles.FLOOR;

                /* Part #2. */
                if (x < cx2)
                    while (x < cx2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        x++;
                    }
                else
                    while (x > cx2)
                    {
                        if (map[x, y] == (byte) Tiles.ROCK)
                            map[x, y] = (byte) Tiles.FLOOR;
                        x--;
                    }

                /* Part #3. */
                while (y < cy2)
                {
                    if (map[x, y] == (byte) Tiles.ROCK)
                        map[x, y] = (byte) Tiles.FLOOR;
                    y++;
                }
            }
            if (map[x, y] == (byte) Tiles.ROCK)
                map[x, y] = (byte) Tiles.FLOOR;
        }

        /// <summary>
        /// Determine whether a given section is set on a border.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool dir_possible(int x, int y, Direction dir)
        {
            return ((dir == Direction.N && y > 0) ||
                    (dir == Direction.S && y < Config.NSECT_H - 1) ||
                    (dir == Direction.W && x > 0) ||
                    (dir == Direction.E && x < Config.NSECT_W - 1));
        }
    }
}