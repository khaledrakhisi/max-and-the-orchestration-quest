import React, { useState, useEffect, useRef } from 'react';
import './index'
import backgroundMusic from './assets/Galactic_Gauntlet.mp3';

declare global {
  interface Window {
    electronAPI?: {
      launchGame: (levelId: number) => void;
      stopGame: () => void;
      onUnityClosed: (callback: () => void) => void;
    };
  }
}

type ViewState = 'MENU' | 'GAME' | 'LEVEL_SELECT' | 'OPTIONS' | 'LEADERBOARDS';

function App() {
  const [currentView, setCurrentView] = useState<ViewState>('MENU');
  const [volume, setVolume] = useState<number>(0.5);
  const [isMuted, setIsMuted] = useState<boolean>(false);
  const audioRef = useRef<HTMLAudioElement>(null);

  // Listen for Unity closing to return to menu
  useEffect(() => {
    if (window.electronAPI) {
      window.electronAPI.onUnityClosed(() => {
        console.log("Unity closed, returning to menu...");
        setCurrentView('MENU');
      });
    }
  }, []);

  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.volume = isMuted ? 0 : volume;
    }
  }, [volume, isMuted]);

  useEffect(() => {
    const handleInteraction = () => {
      if (currentView !== 'GAME' && audioRef.current && audioRef.current.paused) {
        audioRef.current.play().catch(e => console.log("Audio play failed:", e));
      }
    };
    document.addEventListener('click', handleInteraction);
    return () => document.removeEventListener('click', handleInteraction);
  }, [currentView]);

  useEffect(() => {
    if (audioRef.current) {
      if (currentView === 'GAME') {
        audioRef.current.pause();
      } else {
        audioRef.current.play().catch(e => console.log("Audio play failed:", e));
      }
    }
  }, [currentView]);

  const renderMenu = () => (
    <div className="main-menu-container">
      <div className="menu-buttons">
        <button className="retro-btn" onClick={() => {
          if (audioRef.current) {
            audioRef.current.pause();
          }
          setCurrentView('GAME');
          // Tell Electron to launch the .exe (Default to level 1)
          if (window.electronAPI) window.electronAPI.launchGame(1);
        }}>
          START GAME
        </button>
        <button className="retro-btn" onClick={() => setCurrentView('LEVEL_SELECT')}>
          SELECT LEVEL
        </button>
        <button className="retro-btn" onClick={() => setCurrentView('OPTIONS')}>
          OPTIONS
        </button>
        <button className="retro-btn" onClick={() => setCurrentView('LEADERBOARDS')}>
          LEADERBOARDS
        </button>
      </div>
    </div>
  );

  const renderGameMockup = () => (
    <div className="in-game-container">
      <button className="retro-btn back-btn" onClick={() => setCurrentView('MENU')}>
        &lt; BACK
      </button>
      <div className="game-mockup">
        [ UNITY GAME INSTANCE LAUNCHING... ]
      </div>
    </div>
  );

  const renderLevelSelect = () => (
    <div className="view-container">
      <div className="pixel-box" style={{ width: '800px', padding: '40px', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '40px', boxSizing: 'border-box' }}>
        <h2 style={{ color: 'var(--retro-accent)', margin: 0, textShadow: '2px 2px 0 var(--retro-black)' }}>SELECT LEVEL</h2>

        <div style={{ display: 'flex', gap: '30px', justifyContent: 'center', width: '100%' }}>
          {[
            { id: 1, name: 'DOCKER SEAS', status: 'UNLOCKED' },
            { id: 2, name: 'KUBE MOUNTAIN', status: 'LOCKED' },
            { id: 3, name: 'CLOUD FORTRESS', status: 'LOCKED' },
          ].map((level) => (
            <div
              key={level.id}
              className="pixel-box"
              style={{
                flex: 1,
                height: '200px',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '15px',
                cursor: level.status === 'UNLOCKED' ? 'pointer' : 'not-allowed',
                opacity: level.status === 'LOCKED' ? 0.6 : 1,
                transition: 'transform 0.1s',
                backgroundColor: level.status === 'UNLOCKED' ? '#222' : 'var(--retro-black)'
              }}
              onClick={() => {
                if (level.status === 'UNLOCKED') {
                  setCurrentView('GAME');
                }
              }}
            >
              <h3 style={{ margin: 0, fontSize: '1.5rem', color: 'var(--retro-accent)' }}>{level.id}</h3>
              <div style={{ fontSize: '0.8rem', textAlign: 'center', height: '40px', display: 'flex', alignItems: 'center' }}>{level.name}</div>
              <div style={{
                fontSize: '0.7rem',
                color: level.status === 'UNLOCKED' ? '#0f0' : '#f00',
                marginTop: 'auto'
              }}>
                {level.status}
              </div>
            </div>
          ))}
        </div>

        <button className="retro-btn" onClick={() => setCurrentView('MENU')} style={{ marginTop: '20px' }}>
          RETURN TO MENU
        </button>
      </div>
    </div>
  );

  const renderOptions = () => (
    <div className="view-container">
      <div className="pixel-box" style={{ width: '600px', height: '400px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: '40px' }}>
        <h2 style={{ color: 'var(--retro-accent)' }}>OPTIONS</h2>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px', width: '100%' }}>
          <label style={{ fontSize: '1.2rem', color: isMuted ? '#666' : 'inherit' }}>
            MUSIC VOLUME: {Math.round(volume * 100)}%
          </label>
          <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
            <input
              type="range"
              min="0"
              max="1"
              step="0.01"
              value={volume}
              onChange={(e) => setVolume(parseFloat(e.target.value))}
              style={{ width: '300px', cursor: 'pointer', opacity: isMuted ? 0.5 : 1 }}
              disabled={isMuted}
            />
            <button
              className="retro-btn"
              style={{ fontSize: '1.2rem', padding: '10px', minWidth: '50px', display: 'flex', justifyContent: 'center', alignItems: 'center' }}
              onClick={() => setIsMuted(!isMuted)}
              title={isMuted ? 'Unmute' : 'Mute'}
            >
              {isMuted ? '🔇' : '🔊'}
            </button>
          </div>
        </div>
        <button className="retro-btn" onClick={() => setCurrentView('MENU')}>
          RETURN TO MENU
        </button>
      </div>
    </div>
  );

  const mockLeaderboard = [
    { rank: 1, name: 'MAX', score: 999990 },
    { rank: 2, name: 'KHL', score: 852000 },
    { rank: 3, name: 'AAB', score: 720450 },
    { rank: 4, name: 'SAM', score: 504000 },
    { rank: 5, name: 'AAA', score: 100000 },
  ];

  const renderLeaderboards = () => (
    <div className="view-container">
      <div className="pixel-box" style={{ width: '600px', height: '500px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'flex-start', padding: '40px', gap: '20px', boxSizing: 'border-box' }}>
        <h2 style={{ color: 'var(--retro-accent)', margin: 0, textShadow: '2px 2px 0 var(--retro-black)' }}>HIGH SCORES</h2>

        <table style={{ width: '85%', color: 'var(--retro-text)', borderCollapse: 'collapse', marginTop: '10px', fontSize: '1.2rem', textAlign: 'center' }}>
          <thead>
            <tr style={{ borderBottom: '4px solid var(--retro-border)' }}>
              <th style={{ padding: '15px' }}>RANK</th>
              <th style={{ padding: '15px' }}>NAME</th>
              <th style={{ padding: '15px' }}>SCORE</th>
            </tr>
          </thead>
          <tbody>
            {mockLeaderboard.map((entry) => (
              <tr key={entry.rank} style={{ borderBottom: '2px dashed #444' }}>
                <td style={{ padding: '15px' }}>{entry.rank}</td>
                <td style={{ padding: '15px', color: 'var(--retro-accent)' }}>{entry.name}</td>
                <td style={{ padding: '15px' }}>{entry.score.toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div style={{ marginTop: 'auto' }}>
          <button className="retro-btn" onClick={() => setCurrentView('MENU')}>
            RETURN TO MENU
          </button>
        </div>
      </div>
    </div>
  );

  return (
    <>
      <audio ref={audioRef} src={backgroundMusic} loop autoPlay />
      {currentView === 'MENU' && renderMenu()}
      {currentView === 'GAME' && renderGameMockup()}
      {currentView === 'LEVEL_SELECT' && renderLevelSelect()}
      {currentView === 'OPTIONS' && renderOptions()}
      {currentView === 'LEADERBOARDS' && renderLeaderboards()}
    </>
  );
}

export default App;
