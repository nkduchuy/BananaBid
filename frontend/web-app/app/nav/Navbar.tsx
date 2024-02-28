import React from 'react'
import { LuBanana } from 'react-icons/lu'

export default function Navbar() {
  return (
    <header className='
      sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md
    '>
      <div className='flex items-center gap-2 text-3xl font-semibold text-yellow-500'>
        <LuBanana size={34}/>
        <div>BananaBid</div>
      </div>
      <div>Search</div>
      <div>Login</div>
    </header>
  )
}