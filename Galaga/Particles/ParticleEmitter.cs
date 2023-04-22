using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Galaga
{
    public class ParticleEmitter
    {

        private Dictionary<int, Particle> m_particles = new Dictionary<int, Particle>();
        private Texture2D m_texParticle;
        private MyRandom m_random = new MyRandom();

        private TimeSpan m_rate;
        private int m_sourceX;
        private int m_sourceY;
        private int m_sarticleSize;
        private int m_speed;
        private TimeSpan m_lifetime;
        private TimeSpan m_switchover;

        public Vector2 Gravity { get; set; }

        public ParticleEmitter(ContentManager content, TimeSpan rate, int sourceX, int sourceY, int size, int speed, TimeSpan lifetime, TimeSpan wwitchover)
        {
            m_rate = rate;
            m_sourceX = sourceX;
            m_sourceY = sourceY;
            m_sarticleSize = size;
            m_speed = speed;
            m_lifetime = lifetime;
            m_switchover = wwitchover;

            m_texParticle = content.Load<Texture2D>("Images/particle");
            

            this.Gravity = new Vector2(0, 0);
        }
        public int ParticleCount
        {
            get { return m_particles.Count; }
        }

        private TimeSpan m_accumulated = TimeSpan.Zero;

        /// <summary>
        /// Generates new particles, updates the state of existing ones and retires expired particles.
        /// </summary>
        public void update(GameTime gameTime)
        {
            //
            // Generate particles at the specified rate
            m_accumulated += gameTime.ElapsedGameTime;
            while (m_accumulated > m_rate)
            {
                m_accumulated -= m_rate;

                Particle p = new Particle(
                    m_random.Next(),
                    new Vector2(m_sourceX, m_sourceY),
                    m_random.nextCircleVector(),
                    (float)m_random.nextGaussian(m_speed, 1),
                    m_lifetime);

                if (!m_particles.ContainsKey(p.name))
                {
                    m_particles.Add(p.name, p);
                }
            }


            //
            // For any existing particles, update them, if we find ones that have expired, add them
            // to the remove list.
            List<int> removeMe = new List<int>();
            foreach (Particle p in m_particles.Values)
            {
                p.lifetime -= gameTime.ElapsedGameTime;
                if (p.lifetime < TimeSpan.Zero)
                {
                    //
                    // Add to the remove list
                    removeMe.Add(p.name);
                }
                //
                // Update its position
                p.position += (p.direction * p.speed);
                //
                // Have it rotate proportional to its speed
                p.rotation += p.speed / 50.0f;
                //
                // Apply some gravity
                p.direction += this.Gravity;
            }

            //
            // Remove any expired particles
            foreach (int Key in removeMe)
            {
                m_particles.Remove(Key);
            }
        }

        /// <summary>
        /// Renders the active particles
        /// </summary>
        public void draw(SpriteBatch spriteBatch)
        {
            Rectangle r = new Rectangle(0, 0, m_sarticleSize, m_sarticleSize);
            foreach (Particle p in m_particles.Values)
            {
                Texture2D texDraw = m_texParticle;

                r.X = (int)p.position.X;
                r.Y = (int)p.position.Y;
                spriteBatch.Draw(
                    texDraw,
                    r,
                    null,
                    Color.White,
                    p.rotation,
                    new Vector2(texDraw.Width / 2, texDraw.Height / 2),
                    SpriteEffects.None,
                    0);
            }
        }
    }
}
