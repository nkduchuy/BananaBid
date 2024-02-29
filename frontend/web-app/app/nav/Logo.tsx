'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import React from 'react'
import { LuBanana } from 'react-icons/lu'

export default function Logo() {
    const reset = useParamsStore(state => state.reset);

    return (
        <div onClick={reset} className='cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500'>
            <LuBanana size={34} />
            <div>BananaBid</div>
        </div>
    )
}